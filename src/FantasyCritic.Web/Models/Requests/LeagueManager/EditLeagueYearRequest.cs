using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class EditLeagueYearRequest
{
    public EditLeagueYearRequest(Guid leagueID, int year, string? leagueYearName,
        LeagueYearSettingsViewModel leagueYearSettings, DraftSettingsRequest? firstDraft)
    {
        LeagueID = leagueID;
        Year = year;
        LeagueYearName = leagueYearName;
        LeagueYearSettings = leagueYearSettings;
        FirstDraft = firstDraft;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    public string? LeagueYearName { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public DraftSettingsRequest? FirstDraft { get; }

    public Result IsValid() => LeagueYearSettings.IsValid();

    public (LeagueYearParameters settings, DraftParameters? firstDraft) ToDomain(
        IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var parsed = LeagueYearSettings.ToDomain(tagDictionary);
        var settings = new LeagueYearParameters(
            LeagueID, Year, LeagueYearName,
            parsed.StandardGames, parsed.CounterPicks,
            parsed.UnrestrictedReleaseStatusDroppableGames,
            parsed.WillNotReleaseDroppableGames,
            parsed.WillReleaseDroppableGames,
            parsed.DropOnlyDraftGames, parsed.GrantSuperDrops,
            parsed.CounterPicksBlockDrops, parsed.AllowMoveIntoIneligible,
            parsed.MinimumBidAmount, parsed.EnableBids,
            parsed.LeagueTags, parsed.SpecialGameSlots,
            parsed.DraftSystem, parsed.PickupSystem, parsed.ScoringSystem,
            parsed.TradingSystem, parsed.TiebreakSystem, parsed.ReleaseSystem,
            parsed.IneligibleGameSystem, parsed.CounterPickDeadline,
            parsed.MightReleaseDroppableDate, parsed.BidsOnlyBeforeNextScheduledDraft);
        var firstDraftParams = FirstDraft?.ToDomain(0);
        return (settings, firstDraftParams);
    }
}
