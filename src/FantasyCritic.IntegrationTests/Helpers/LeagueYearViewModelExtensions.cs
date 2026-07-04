using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Mirrors <c>LeagueYear.DraftForPublisherDisplayOrder</c> — the draft whose pick order
    /// should drive publisher/standings display.
    /// </summary>
    public static LeagueDraftViewModel DisplayOrderDraft(this LeagueYearViewModel leagueYear)
    {
        var lastCompletedDraft = leagueYear.Drafts.LastOrDefault(d => d.PlayStatus == "DraftFinal");
        if (lastCompletedDraft is null)
        {
            return leagueYear.Drafts.First();
        }

        var nextUncompletedDraftWithOrderSet = leagueYear.Drafts.FirstOrDefault(d => d.DraftOrderSet && d.PlayStatus == "NotStartedDraft");
        if (nextUncompletedDraftWithOrderSet is not null)
        {
            return nextUncompletedDraftWithOrderSet;
        }

        return lastCompletedDraft;
    }

    public static IReadOnlyList<Guid> PublisherIDsInDisplayOrder(this LeagueYearViewModel leagueYear) =>
        leagueYear.DisplayOrderDraft().PublisherDraftInfo
            .OrderBy(i => i.DraftPosition)
            .Select(i => i.PublisherID)
            .ToList();
}
