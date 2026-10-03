using System.IO;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Lib.Utilities;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

//A weekly copy of the database kept outside AWS, apart from the pre-bids RDS snapshot, which is the quick restore when bids go wrong.
internal class ArchiveDatabaseJobHandler : IFantasyCriticCronJobHandler
{
    private const string RunAgain = "Start an Archive Database job from the admin console once it's fixed.";

    private readonly IDatabaseDumper _databaseDumper;
    private readonly IReadOnlyList<IDatabaseArchiveLocation> _archiveLocations;
    private readonly EmailSendingService _emailSendingService;
    private readonly IClock _clock;
    private readonly ILogger<ArchiveDatabaseJobHandler> _logger;

    public ArchiveDatabaseJobHandler(IDatabaseDumper databaseDumper, IEnumerable<IDatabaseArchiveLocation> archiveLocations,
        EmailSendingService emailSendingService, IClock clock, ILogger<ArchiveDatabaseJobHandler> logger)
    {
        _databaseDumper = databaseDumper;
        _archiveLocations = archiveLocations.ToList();
        _emailSendingService = emailSendingService;
        _clock = clock;
        _logger = logger;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.ArchiveDatabase;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.Independent;

    //Saturday night into Sunday, after bids run.
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(IsoDayOfWeek.Sunday, new LocalTime(3, 0));

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        Result result;
        try
        {
            result = await ArchiveDatabase(context, cancellationToken);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            await SendStoppedShortEmail("Database archive cancelled", ex, [$"Job {context.Job.JobID} was cancelled before finishing.", RunAgain]);
            throw;
        }
        catch (Exception ex)
        {
            await SendStoppedShortEmail("Database archive failed", ex,
                [$"Job {context.Job.JobID} threw before finishing: {ex.GetType().Name}: {ex.Message}", RunAgain]);
            throw;
        }

        if (result.IsFailure)
        {
            try
            {
                await _emailSendingService.SendAdminNotification("Database archive failed", [$"Job {context.Job.JobID}: {result.Error}", RunAgain]);
            }
            catch (Exception emailException)
            {
                throw new InvalidOperationException($"The database archive failed ({result.Error}), and the email about it failed.", emailException);
            }
        }

        return result;
    }

    private async Task<Result> ArchiveDatabase(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant();
        var fileName = BackupRemoteKeyBuilder.BuildFileName(_databaseDumper.InstanceName, now);
        var key = BackupRemoteKeyBuilder.Build(_databaseDumper.InstanceName, now, fileName);
        var localFilePath = Path.Combine(Path.GetTempPath(), fileName);

        try
        {
            await context.AddTemporaryStatus("Dumping the database.");
            var dumpResult = await _databaseDumper.DumpToGzipFile(localFilePath, cancellationToken);
            if (dumpResult.IsFailure)
            {
                await context.AppendDetailedStatus("Dump failed.");
                return Result.Failure($"Dump failed: {dumpResult.Error}");
            }

            var megabytes = new FileInfo(localFilePath).Length / (1024.0 * 1024.0);
            _logger.LogInformation("Dumped the database to {FileName} ({Megabytes:F1} MB).", fileName, megabytes);
            await context.AppendDetailedStatus($"Dumped {fileName} ({megabytes:F1} MB).");

            //Every location gets its try, so one being down doesn't cost the archive in the others.
            List<string> failures = [];
            foreach (var location in _archiveLocations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await context.AddTemporaryStatus($"Uploading to {location.Name}.");
                try
                {
                    await location.Upload(localFilePath, key, cancellationToken);
                    await context.AppendDetailedStatus($"Uploaded to {location.Name}.");
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Uploading the database archive to {Location} failed.", location.Name);
                    failures.Add($"{location.Name}: {ex.GetType().Name}: {ex.Message}");
                    await context.AppendDetailedStatus($"Upload to {location.Name} failed.");
                }
            }

            if (failures.Any())
            {
                return Result.Failure($"Upload failed for {string.Join("; ", failures)}");
            }

            return Result.Success();
        }
        finally
        {
            File.Delete(localFilePath);
        }
    }

    //If the email fails too, both errors go on the job, so the original isn't lost behind the email's.
    private async Task SendStoppedShortEmail(string subject, Exception ex, IReadOnlyList<string> lines)
    {
        try
        {
            await _emailSendingService.SendAdminNotification(subject, lines);
        }
        catch (Exception emailException)
        {
            throw new AggregateException("The database archive job stopped short, and the email about it failed.", ex, emailException);
        }
    }
}
