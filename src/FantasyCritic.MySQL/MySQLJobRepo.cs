using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using FantasyCritic.MySQL.Entities;

namespace FantasyCritic.MySQL;

public class MySQLJobRepo : IJobRepo
{
    //ErrorMessage is TEXT, capped at 65,535 bytes. utf8mb4 is up to 4 bytes a character, so this always fits.
    //An oversized stack trace must not turn "record the failure" into a second failure.
    private const int MaxErrorMessageLength = 16_000;

    private const string JobSelectSQL =
        """
        SELECT tbl_job.*,
               tbl_job_type.RunType,
               createdByUser.DisplayName AS CreatedByUserDisplayName,
               createdByUser.EmailAddress AS CreatedByUserEmailAddress
        FROM tbl_job
        JOIN tbl_job_type ON tbl_job.JobType = tbl_job_type.Name
        LEFT JOIN tbl_user createdByUser ON tbl_job.CreatedByUserID = createdByUser.UserID
        """;

    private static readonly IReadOnlyList<string> IncompleteStatuses =
    [
        FantasyCriticJobStatus.Queued.Value,
        FantasyCriticJobStatus.Running.Value,
        FantasyCriticJobStatus.Cancelling.Value
    ];

    //A started job is Running, or Cancelling if an admin asked to stop it. Either way the runner still owns it.
    private static readonly IReadOnlyList<string> StartedStatuses =
    [
        FantasyCriticJobStatus.Running.Value,
        FantasyCriticJobStatus.Cancelling.Value
    ];

    private readonly string _connectionString;

    public MySQLJobRepo(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public async Task<IReadOnlyList<FantasyCriticJob>> GetRecentJobs(int count)
    {
        var sql = $"{JobSelectSQL} ORDER BY tbl_job.CreatedAt DESC LIMIT @count;";

        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<JobEntity>(sql, new { count });
        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<FantasyCriticJob>> GetIncompleteJobs()
    {
        var sql = $"{JobSelectSQL} WHERE tbl_job.Status IN @statuses ORDER BY tbl_job.CreatedAt;";

        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<JobEntity>(sql, new { statuses = IncompleteStatuses });
        return entities.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<FantasyCriticJobTypeWithRunType>> GetJobTypeRunTypes()
    {
        const string sql = "SELECT Name, RunType FROM tbl_job_type;";

        await using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.QueryAsync<JobTypeRunTypeEntity>(sql);

        //A row with no matching type in code is skipped rather than thrown on: it can never be enqueued, so it has nothing to schedule.
        var jobTypeRunTypes = new List<FantasyCriticJobTypeWithRunType>();
        foreach (var row in rows)
        {
            var jobType = FantasyCriticJobType.TryFromValue(row.Name);
            var runType = FantasyCriticJobRunType.TryFromValue(row.RunType);
            if (jobType is not null && runType is not null)
            {
                jobTypeRunTypes.Add(new FantasyCriticJobTypeWithRunType(jobType, runType));
            }
        }

        return jobTypeRunTypes;
    }

    public async Task<IReadOnlyDictionary<FantasyCriticJobType, Instant>> GetLastScheduledTimes()
    {
        //Answered from UQ_tbl_job_scheduledslot (JobType, ScheduledFor) without reading rows.
        const string sql =
            """
            SELECT JobType, MAX(ScheduledFor) AS LastScheduledFor
            FROM tbl_job
            WHERE ScheduledFor IS NOT NULL
            GROUP BY JobType;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.QueryAsync<LastScheduledJobEntity>(sql);

        var lastScheduledTimes = new Dictionary<FantasyCriticJobType, Instant>();
        foreach (var row in rows)
        {
            var jobType = FantasyCriticJobType.TryFromValue(row.JobType);
            if (jobType is not null)
            {
                lastScheduledTimes[jobType] = row.LastScheduledFor;
            }
        }

        return lastScheduledTimes;
    }

    public async Task<bool> CreateJob(FantasyCriticJob job)
    {
        const string sql =
            """
            INSERT INTO tbl_job (JobID, JobType, CreatedByUserID, Status, DetailedStatus, ErrorMessage, ScheduledFor, CreatedAt, StartedAt, FinishedAt)
            VALUES (@jobID, @jobType, @createdByUserID, @status, @detailedStatus, @errorMessage, @scheduledFor, @createdAt, @startedAt, @finishedAt);
            """;

        var parameters = new
        {
            jobID = job.JobID,
            jobType = job.Type.Value,
            createdByUserID = job.CreatedByUser?.UserID,
            status = job.Status.Value,
            detailedStatus = job.DetailedStatus,
            errorMessage = job.ErrorMessage,
            scheduledFor = job.ScheduledFor,
            createdAt = job.CreatedAt,
            startedAt = job.StartedAt,
            finishedAt = job.FinishedAt
        };

        await using var connection = new MySqlConnection(_connectionString);
        try
        {
            await connection.ExecuteAsync(sql, parameters);
            return true;
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry && job.ScheduledFor is not null)
        {
            //UQ_tbl_job_scheduledslot: another scheduler, or this one before a restart, already enqueued this slot.
            return false;
        }
    }

    public async Task<bool> StartJob(FantasyCriticJob job, Instant startTime)
    {
        const string sql =
            """
            UPDATE tbl_job SET Status = @running, StartedAt = @startTime
            WHERE JobID = @jobID AND Status = @queued;
            """;

        var parameters = new
        {
            jobID = job.JobID,
            startTime,
            running = FantasyCriticJobStatus.Running.Value,
            queued = FantasyCriticJobStatus.Queued.Value
        };

        await using var connection = new MySqlConnection(_connectionString);
        var rowsUpdated = await connection.ExecuteAsync(sql, parameters);
        return rowsUpdated >= 1;
    }

    public async Task CompleteJob(FantasyCriticJob job, Instant finishTime)
    {
        await FinishStartedJob(job, FantasyCriticJobStatus.Complete, finishTime, errorMessage: null);
    }

    public async Task UpdateDetailedStatusForJob(FantasyCriticJob job, string detailedStatus)
    {
        const string sql = "UPDATE tbl_job SET DetailedStatus = @detailedStatus WHERE JobID = @jobID;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { jobID = job.JobID, detailedStatus });
    }

    public async Task ErrorJob(FantasyCriticJob job, string errorMessage, Instant finishTime)
    {
        if (errorMessage.Length > MaxErrorMessageLength)
        {
            errorMessage = errorMessage[..MaxErrorMessageLength];
        }

        await FinishStartedJob(job, FantasyCriticJobStatus.Error, finishTime, errorMessage);
    }

    public async Task<bool> CancelJob(FantasyCriticJob job, Instant cancellationTime)
    {
        //The canceller's "never started" path. StartedAt IS NULL is what keeps it from settling a job the runner already owns.
        const string sql =
            """
            UPDATE tbl_job SET Status = @cancelled, FinishedAt = @cancellationTime
            WHERE JobID = @jobID AND Status = @cancelling AND StartedAt IS NULL;
            """;

        var parameters = new
        {
            jobID = job.JobID,
            cancellationTime,
            cancelled = FantasyCriticJobStatus.Cancelled.Value,
            cancelling = FantasyCriticJobStatus.Cancelling.Value
        };

        await using var connection = new MySqlConnection(_connectionString);
        var rowsUpdated = await connection.ExecuteAsync(sql, parameters);
        return rowsUpdated >= 1;
    }

    public async Task<bool> CancelQueuedJob(FantasyCriticJob job, string reason, Instant cancellationTime)
    {
        //Conditional on Queued, the same condition the runner's claim uses, so exactly one of the two wins.
        const string sql =
            """
            UPDATE tbl_job SET Status = @cancelled, DetailedStatus = @reason, FinishedAt = @cancellationTime
            WHERE JobID = @jobID AND Status = @queued;
            """;

        var parameters = new
        {
            jobID = job.JobID,
            reason,
            cancellationTime,
            cancelled = FantasyCriticJobStatus.Cancelled.Value,
            queued = FantasyCriticJobStatus.Queued.Value
        };

        await using var connection = new MySqlConnection(_connectionString);
        var rowsUpdated = await connection.ExecuteAsync(sql, parameters);
        return rowsUpdated >= 1;
    }

    public async Task CancelInProgressJob(FantasyCriticJob job, Instant cancellationTime)
    {
        await FinishStartedJob(job, FantasyCriticJobStatus.CancelledInProgress, cancellationTime, errorMessage: null);
    }

    private async Task FinishStartedJob(FantasyCriticJob job, FantasyCriticJobStatus finalStatus, Instant finishTime, string? errorMessage)
    {
        //Only a started job can finish, so a terminal status is never overwritten by a late or duplicate write.
        const string sql =
            """
            UPDATE tbl_job SET Status = @finalStatus, ErrorMessage = @errorMessage, FinishedAt = @finishTime
            WHERE JobID = @jobID AND Status IN @startedStatuses;
            """;

        var parameters = new
        {
            jobID = job.JobID,
            finalStatus = finalStatus.Value,
            errorMessage,
            finishTime,
            startedStatuses = StartedStatuses
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, parameters);
    }
}
