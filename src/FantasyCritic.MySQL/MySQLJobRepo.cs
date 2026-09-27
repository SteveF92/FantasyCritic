using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Identity;
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
               tbl_job_type.Severity,
               createdByUser.DisplayName AS CreatedByUserDisplayName,
               createdByUser.EmailAddress AS CreatedByUserEmailAddress,
               cancelledByUser.DisplayName AS CancelledByUserDisplayName,
               cancelledByUser.EmailAddress AS CancelledByUserEmailAddress
        FROM tbl_job
        JOIN tbl_job_type ON tbl_job.JobType = tbl_job_type.Name
        LEFT JOIN tbl_user createdByUser ON tbl_job.CreatedByUserID = createdByUser.UserID
        LEFT JOIN tbl_user cancelledByUser ON tbl_job.CancelledByUserID = cancelledByUser.UserID
        """;

    private static readonly IReadOnlyList<string> IncompleteStatuses =
    [
        FantasyCriticJobStatus.Queued.Value,
        FantasyCriticJobStatus.Running.Value,
        FantasyCriticJobStatus.Cancelling.Value
    ];

    //The statuses an external cancellation request can still act on. Anything past Cancelling is already settled.
    private static readonly IReadOnlyList<string> CancellableStatuses =
    [
        FantasyCriticJobStatus.Queued.Value,
        FantasyCriticJobStatus.Running.Value
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

    public async Task<FantasyCriticJob?> GetJob(Guid jobID)
    {
        var sql = $"{JobSelectSQL} WHERE tbl_job.JobID = @jobID;";

        await using var connection = new MySqlConnection(_connectionString);
        var entity = await connection.QuerySingleOrDefaultAsync<JobEntity>(sql, new { jobID });
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<FantasyCriticJob>> GetJobs(int page, int count, FantasyCriticJobFilter filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("count", count);
        parameters.Add("offset", (Math.Max(page, 1) - 1) * count);

        //An empty list means "no restriction", so it adds no condition at all rather than an IN () that matches nothing.
        var conditions = new List<string>();
        void AddCondition(string column, string sqlOperator, string parameterName, IEnumerable<string> values)
        {
            var valueList = values.ToList();
            if (valueList.Count == 0)
            {
                return;
            }

            conditions.Add($"{column} {sqlOperator} @{parameterName}");
            parameters.Add(parameterName, valueList);
        }

        AddCondition("tbl_job.JobType", "IN", "jobTypes", filter.JobTypes.Select(x => x.Value));
        AddCondition("tbl_job.JobType", "NOT IN", "excludedJobTypes", filter.ExcludedJobTypes.Select(x => x.Value));
        AddCondition("tbl_job.Status", "IN", "statuses", filter.Statuses.Select(x => x.Value));
        AddCondition("tbl_job.Status", "NOT IN", "excludedStatuses", filter.ExcludedStatuses.Select(x => x.Value));

        var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        var sql = $"{JobSelectSQL} {whereClause} ORDER BY tbl_job.CreatedAt DESC LIMIT @count OFFSET @offset;";

        await using var connection = new MySqlConnection(_connectionString);
        var entities = await connection.QueryAsync<JobEntity>(sql, parameters);
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
        const string sql = "SELECT Name, RunType, Severity FROM tbl_job_type;";

        await using var connection = new MySqlConnection(_connectionString);
        var rows = await connection.QueryAsync<JobTypeRunTypeEntity>(sql);

        //A row with no matching type in code is skipped rather than thrown on: it can never be enqueued, so it has nothing to schedule.
        var jobTypeRunTypes = new List<FantasyCriticJobTypeWithRunType>();
        foreach (var row in rows)
        {
            var jobType = FantasyCriticJobType.TryFromValue(row.Name);
            var runType = FantasyCriticJobRunType.TryFromValue(row.RunType);
            var severity = FantasyCriticJobSeverity.TryFromValue(row.Severity);
            if (jobType is not null && runType is not null && severity is not null)
            {
                jobTypeRunTypes.Add(new FantasyCriticJobTypeWithRunType(jobType, runType, severity));
            }
        }

        return jobTypeRunTypes;
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

    public async Task<Result<FantasyCriticJob>> EnqueueJob(FantasyCriticJobType jobType, IMinimalFantasyCriticUser createdByUser, Instant createdAt)
    {
        var jobTypeRunTypes = await GetJobTypeRunTypes();
        var jobTypeWithRunType = jobTypeRunTypes.SingleOrDefault(x => x.JobType.Equals(jobType));
        if (jobTypeWithRunType is null)
        {
            throw new Exception($"Job Type {jobType} not found in database.");
        }

        if (!jobTypeWithRunType.RunType.AllowsManual)
        {
            return Result.Failure<FantasyCriticJob>($"{jobType} cannot be run manually: its RunType is {jobTypeWithRunType.RunType}.");
        }

        var job = new FantasyCriticJob(Guid.NewGuid(), jobTypeWithRunType, createdByUser, FantasyCriticJobStatus.Queued,
            detailedStatus: null, errorMessage: null, scheduledFor: null, createdAt: createdAt, startedAt: null, finishedAt: null,
            cancelledAt: null, cancelledByUser: null);

        //The worker runs jobs one at a time, so a second press would run the job again as soon as the first finished -
        //double emails, or actions processed twice. The check is part of the insert so two presses can't both pass it.
        const string sql =
            """
            INSERT INTO tbl_job (JobID, JobType, CreatedByUserID, Status, CreatedAt)
            SELECT @jobID, @jobType, @createdByUserID, @queued, @createdAt FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM tbl_job WHERE JobType = @jobType AND Status IN @incompleteStatuses);
            """;

        var parameters = new
        {
            jobID = job.JobID,
            jobType = jobType.Value,
            createdByUserID = createdByUser.UserID,
            queued = FantasyCriticJobStatus.Queued.Value,
            createdAt,
            incompleteStatuses = IncompleteStatuses
        };

        await using var connection = new MySqlConnection(_connectionString);
        var rowsInserted = await connection.ExecuteAsync(sql, parameters);
        if (rowsInserted == 0)
        {
            return Result.Failure<FantasyCriticJob>($"{jobType} is already queued or running.");
        }

        return job;
    }

    public async Task<bool> StartJob(FantasyCriticJob job, Instant startTime)
    {
        //The flag is part of the claim itself, so turning it off and a job starting cannot race:
        //once it reads off, no claim succeeds, and "nothing is Running" stays true.
        const string sql =
            """
            UPDATE tbl_job SET Status = @running, StartedAt = @startTime
            WHERE JobID = @jobID AND Status = @queued
            AND EXISTS (SELECT 1 FROM tbl_meta_systemwidesettings WHERE WorkerShouldPullNewJobs = 1);
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

    public async Task<bool> RequestCancellation(FantasyCriticJob job, IMinimalFantasyCriticUser cancelledByUser, Instant requestedAt)
    {
        //Conditional on Queued/Running so a job that already finished, or one already Cancelling, is left alone.
        //The worker's cancellation loop is what actually settles the job from here.
        const string sql =
            """
            UPDATE tbl_job SET Status = @cancelling, CancelledAt = @requestedAt, CancelledByUserID = @cancelledByUserID
            WHERE JobID = @jobID AND Status IN @cancellableStatuses;
            """;

        var parameters = new
        {
            jobID = job.JobID,
            cancelling = FantasyCriticJobStatus.Cancelling.Value,
            cancellableStatuses = CancellableStatuses,
            requestedAt,
            cancelledByUserID = cancelledByUser.UserID
        };

        await using var connection = new MySqlConnection(_connectionString);
        var rowsUpdated = await connection.ExecuteAsync(sql, parameters);
        return rowsUpdated >= 1;
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
