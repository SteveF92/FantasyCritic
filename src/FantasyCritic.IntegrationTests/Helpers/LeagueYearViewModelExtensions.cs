using System.Linq;
using FantasyCritic.ApiClient;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Per-draft accessors mirroring the frontend <c>leagueMixin</c> helpers.
/// </summary>
internal static class LeagueYearViewModelExtensions
{
    public static LeagueDraftViewModel? FirstDraft(this LeagueYearViewModel leagueYear) =>
        leagueYear.Drafts.FirstOrDefault();

    public static LeagueDraftViewModel? PendingDraft(this LeagueYearViewModel leagueYear) =>
        leagueYear.Drafts.FirstOrDefault(d => d.PlayStatus == "NotStartedDraft");

    public static LeagueDraftViewModel? ActiveDraft(this LeagueYearViewModel leagueYear) =>
        leagueYear.Drafts.FirstOrDefault(d => d.DraftIsActive || d.DraftIsPaused);
}
