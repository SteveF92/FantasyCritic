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

    public (LeagueYearParameters settings, DraftParameters? firstDraft) ToDomain(LeagueYear existingLeagueYear, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        var parsed = LeagueYearSettings.ToDomain(tagDictionary);

        var reconciledSlots = ReconcileOldAndNewSlots(existingLeagueYear.Options.LeagueSlots, parsed.LeagueSlots);

        var settings = new LeagueYearParameters(
            LeagueID, Year, LeagueYearName,
            parsed.UnrestrictedReleaseStatusDroppableGames,
            parsed.WillNotReleaseDroppableGames,
            parsed.WillReleaseDroppableGames,
            parsed.DropOnlyDraftGames, parsed.GrantSuperDrops,
            parsed.CounterPicksBlockDrops, parsed.AllowMoveIntoIneligible,
            parsed.MinimumBidAmount, parsed.EnableBids,
            parsed.LeagueTags, reconciledSlots,
            parsed.PickupSystem, parsed.ScoringSystem,
            parsed.TradingSystem, parsed.TiebreakSystem, parsed.ReleaseSystem,
            parsed.IneligibleGameSystem, parsed.CounterPickDeadline,
            parsed.MightReleaseDroppableDate, parsed.BidsOnlyBeforeNextScheduledDraft);
        var firstDraftParams = FirstDraft?.ToDomain(0);
        return (settings, firstDraftParams);
    }

    private static IReadOnlyList<LeagueSlot> ReconcileOldAndNewSlots(IReadOnlyList<LeagueSlot> existingLeagueSlots, IReadOnlyList<LeagueSlot> newLeagueSlots)
    {
        //Just purely creating new slots will mean changing the LeagueSlotIDs on every edit
        //This will get easier when we extend the new logic all the way to the front end, but for now, we need to figure out what slots haven't changed and we should use the old ones.
        throw new NotImplementedException();
    }
}
