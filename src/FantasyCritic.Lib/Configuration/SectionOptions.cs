namespace FantasyCritic.Lib.Configuration;

public sealed record ConnectionStringsOptions : IOptionsSection
{
    public required string DefaultConnection { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(DefaultConnection)}", DefaultConnection)
            .Paths;
    }
}

/// <summary>
/// The migrator's view of ConnectionStrings: it connects as the admin user, which can change the schema.
/// </summary>
public sealed record AdminConnectionStringsOptions : IOptionsSection
{
    public required string AdminConnection { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(AdminConnection)}", AdminConnection)
            .Paths;
    }
}

/// <summary>
/// Every host reads the region, to find the secret store.
/// </summary>
public record AwsRegionOptions : IOptionsSection
{
    public required string Region { get; init; }

    public virtual IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(Region)}", Region)
            .Paths;
    }
}

/// <summary>
/// The hosts that take database snapshots also need the RDS instance.
/// </summary>
public sealed record AwsOptions : AwsRegionOptions
{
    public required string RdsInstanceName { get; init; }

    public override IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        var own = new MissingConfiguration(environment)
            .Value($"{path}:{nameof(RdsInstanceName)}", RdsInstanceName, FantasyCriticEnvironment.Beta)
            .Paths;
        return [.. base.Validate(path, environment), .. own];
    }
}

public sealed record PostmarkOptions : IOptionsSection
{
    public required string ApiKey { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ApiKey)}", ApiKey, FantasyCriticEnvironment.Beta)
            .Paths;
    }
}

public sealed record OpenCriticOptions : IOptionsSection
{
    public required string ApiKey { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ApiKey)}", ApiKey, FantasyCriticEnvironment.Beta)
            .Paths;
    }
}

/// <summary>
/// Web registers the login providers only in production, so their keys are required only there.
/// </summary>
public sealed record OAuthClientOptions : IOptionsSection
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ClientId)}", ClientId, FantasyCriticEnvironment.Production)
            .Value($"{path}:{nameof(ClientSecret)}", ClientSecret, FantasyCriticEnvironment.Production)
            .Paths;
    }
}

/// <summary>
/// Patreon's keys, for both the login provider and the API client, which refreshes its tokens with the client secret.
/// Patreon is a login provider first, so these live under Authentication:Patreon.
/// </summary>
public sealed record PatreonOptions : IOptionsSection
{
    public required string ClientId { get; init; }
    public required string CampaignId { get; init; }
    public required string ClientSecret { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ClientId)}", ClientId, FantasyCriticEnvironment.Production)
            .Value($"{path}:{nameof(CampaignId)}", CampaignId, FantasyCriticEnvironment.Production)
            .Value($"{path}:{nameof(ClientSecret)}", ClientSecret, FantasyCriticEnvironment.Production)
            .Paths;
    }
}

public sealed record AuthenticationOptions : IOptionsSection
{
    public required OAuthClientOptions Google { get; init; }
    public required OAuthClientOptions Microsoft { get; init; }
    public required OAuthClientOptions Twitch { get; init; }
    public required PatreonOptions Patreon { get; init; }
    public required OAuthClientOptions Discord { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Section($"{path}:{nameof(Google)}", Google)
            .Section($"{path}:{nameof(Microsoft)}", Microsoft)
            .Section($"{path}:{nameof(Twitch)}", Twitch)
            .Section($"{path}:{nameof(Patreon)}", Patreon)
            .Section($"{path}:{nameof(Discord)}", Discord)
            .Paths;
    }
}

public sealed record DiscordOptions : IOptionsSection
{
    public required string BotToken { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(BotToken)}", BotToken, FantasyCriticEnvironment.Beta)
            .Paths;
    }
}

/// <summary>
/// The worker's ArchiveDatabase job: the database it dumps, as a read-only user, and the buckets the dump goes to. Only
/// production archives, so elsewhere these may stay placeholders.
/// </summary>
public sealed record DatabaseArchiveOptions : IOptionsSection
{
    public required string ConnectionString { get; init; }
    public required S3ArchiveOptions S3 { get; init; }
    public required GoogleCloudArchiveOptions GoogleCloud { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ConnectionString)}", ConnectionString, FantasyCriticEnvironment.Production)
            .Section($"{path}:{nameof(S3)}", S3)
            .Section($"{path}:{nameof(GoogleCloud)}", GoogleCloud)
            .Paths;
    }
}

public sealed record S3ArchiveOptions : IOptionsSection
{
    public required string Bucket { get; init; }
    public required string Prefix { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(Bucket)}", Bucket, FantasyCriticEnvironment.Production)
            .Present($"{path}:{nameof(Prefix)}", Prefix)
            .Paths;
    }
}

public sealed record GoogleCloudArchiveOptions : IOptionsSection
{
    public required string Bucket { get; init; }
    public required string Prefix { get; init; }

    /// <summary>
    /// The Workload Identity Federation credential configuration, as JSON. It holds no key: it tells Google's library to
    /// sign in with the host's AWS role. Left the placeholder, the library falls back to Application Default Credentials.
    /// </summary>
    public required string CredentialConfiguration { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(Bucket)}", Bucket, FantasyCriticEnvironment.Production)
            .Present($"{path}:{nameof(Prefix)}", Prefix)
            .Value($"{path}:{nameof(CredentialConfiguration)}", CredentialConfiguration, FantasyCriticEnvironment.Production)
            .Paths;
    }
}

public sealed record ServiceHealthOptions : IOptionsSection
{
    public required string WorkerUrl { get; init; }
    public required string DiscordBotUrl { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(WorkerUrl)}", WorkerUrl)
            .Value($"{path}:{nameof(DiscordBotUrl)}", DiscordBotUrl)
            .Paths;
    }
}

public sealed record GrafanaOptions : IOptionsSection
{
    public required LokiOptions Loki { get; init; }

    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Section($"{path}:{nameof(Loki)}", Loki)
            .Paths;
    }
}

/// <summary>
/// Where people read the logs Loki holds, for the links the admin console puts beside each job.
/// </summary>
public sealed record GrafanaLogsOptions : IOptionsSection
{
    public required string Url { get; init; }
    public required string DataSource { get; init; }

    //An empty Url turns the links off.
    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Present($"{path}:{nameof(Url)}", Url)
            .Present($"{path}:{nameof(DataSource)}", DataSource)
            .Paths;
    }
}

public sealed record LokiOptions : IOptionsSection
{
    public required string Uri { get; init; }
    public required string UserId { get; init; }
    public required string ApiToken { get; init; }

    /// <summary>
    /// Sent to Loki as they are written: a key here is a label name, which is case-sensitive, not a configuration key.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Labels { get; init; }

    //Empty strings turn the sink off, so the keys only have to be there.
    public IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Present($"{path}:{nameof(Uri)}", Uri)
            .Present($"{path}:{nameof(UserId)}", UserId)
            .Present($"{path}:{nameof(ApiToken)}", ApiToken)
            .Present($"{path}:{nameof(Labels)}", Labels)
            .Paths;
    }
}
