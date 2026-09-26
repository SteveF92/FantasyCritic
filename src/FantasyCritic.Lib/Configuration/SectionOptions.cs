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
/// What the Patreon API client needs. Patreon is also a login provider, so these live under Authentication:Patreon.
/// </summary>
public record PatreonOptions : IOptionsSection
{
    public required string ClientId { get; init; }
    public required string CampaignId { get; init; }

    public virtual IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        return new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ClientId)}", ClientId, FantasyCriticEnvironment.Production)
            .Value($"{path}:{nameof(CampaignId)}", CampaignId, FantasyCriticEnvironment.Production)
            .Paths;
    }
}

/// <summary>
/// Web's view of Authentication:Patreon: the API client's keys, plus the secret it needs to log people in.
/// </summary>
public sealed record PatreonAuthOptions : PatreonOptions
{
    public required string ClientSecret { get; init; }

    public override IReadOnlyList<string> Validate(string path, FantasyCriticEnvironment environment)
    {
        var own = new MissingConfiguration(environment)
            .Value($"{path}:{nameof(ClientSecret)}", ClientSecret, FantasyCriticEnvironment.Production)
            .Paths;
        return [.. base.Validate(path, environment), .. own];
    }
}

public sealed record AuthenticationOptions : IOptionsSection
{
    public required OAuthClientOptions Google { get; init; }
    public required OAuthClientOptions Microsoft { get; init; }
    public required OAuthClientOptions Twitch { get; init; }
    public required PatreonAuthOptions Patreon { get; init; }
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
