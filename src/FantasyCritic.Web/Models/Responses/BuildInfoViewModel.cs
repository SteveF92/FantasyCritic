using FantasyCritic.Web.Utilities;

namespace FantasyCritic.Web.Models.Responses;

public class BuildInfoViewModel
{
    public BuildInfoViewModel(BuildInfo domain)
    {
        IsLocalBuild = domain.IsLocalBuild;
        ReleaseID = domain.ReleaseID;
        DeployedEnvironment = domain.DeployedEnvironment;
        CommitHash = domain.CommitHash;
        ShortCommitHash = domain.ShortCommitHash;
        CommitUrl = domain.CommitUrl;
        GitRef = domain.GitRef;
        CommitDate = domain.CommitDate;
        BuiltAt = domain.BuiltAt;
        DeployedAt = domain.DeployedAt;
        BuildRunUrl = domain.BuildRunUrl;
        ProcessStartedAt = domain.ProcessStartedAt;
    }

    public bool IsLocalBuild { get; }
    public string? ReleaseID { get; }
    public string? DeployedEnvironment { get; }
    public string? CommitHash { get; }
    public string? ShortCommitHash { get; }
    public string? CommitUrl { get; }
    public string? GitRef { get; }
    public Instant? CommitDate { get; }
    public Instant? BuiltAt { get; }
    public Instant? DeployedAt { get; }
    public string? BuildRunUrl { get; }
    public Instant ProcessStartedAt { get; }
}
