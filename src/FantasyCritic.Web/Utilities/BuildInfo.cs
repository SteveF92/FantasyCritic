namespace FantasyCritic.Web.Utilities;

/// <summary>
/// Build and deploy provenance for the running process, read from the RELEASE file that the deploy
/// pipeline writes into the release directory. A local build has no such file, so everything except
/// <see cref="ProcessStartedAt"/> is null there.
/// </summary>
public record BuildInfo(string? ReleaseID, string? DeployedEnvironment, string? CommitHash, string? CommitUrl,
    string? GitRef, Instant? CommitDate, Instant? BuiltAt, Instant? DeployedAt, string? BuildRunUrl,
    Instant ProcessStartedAt)
{
    public bool IsLocalBuild => CommitHash is null;

    public string? ShortCommitHash => CommitHash is null ? null : CommitHash[..Math.Min(7, CommitHash.Length)];
}
