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
        return
        [
            ..ReconcileSlotsOfKind(existingLeagueSlots.Where(x => !x.CounterPick), newLeagueSlots.Where(x => !x.CounterPick)),
            ..ReconcileSlotsOfKind(existingLeagueSlots.Where(x => x.CounterPick), newLeagueSlots.Where(x => x.CounterPick)),
        ];
    }

    //A new slot first takes an existing slot with the same tags, so games stay with their tags when special slots are added or removed.
    //Any new slot left over takes a leftover existing slot and retags it, so converting a slot is an update rather than a deletion.
    private static IReadOnlyList<LeagueSlot> ReconcileSlotsOfKind(IEnumerable<LeagueSlot> existingSlots, IEnumerable<LeagueSlot> newSlots)
    {
        var unclaimedExistingSlots = existingSlots.OrderBy(x => x.SlotNumber).ToList();
        var orderedNewSlots = newSlots.OrderBy(x => x.SlotNumber).ToList();
        Dictionary<LeagueSlot, LeagueSlot> existingSlotByNewSlot = [];

        foreach (var newSlot in orderedNewSlots)
        {
            var existingSlotWithSameTags = unclaimedExistingSlots.FirstOrDefault(x => x.RequiredTags.ToHashSet().SetEquals(newSlot.RequiredTags));
            if (existingSlotWithSameTags is not null)
            {
                existingSlotByNewSlot.Add(newSlot, existingSlotWithSameTags);
                unclaimedExistingSlots.Remove(existingSlotWithSameTags);
            }
        }

        foreach (var newSlot in orderedNewSlots.Where(x => !existingSlotByNewSlot.ContainsKey(x)))
        {
            var leftoverExistingSlot = unclaimedExistingSlots.FirstOrDefault();
            if (leftoverExistingSlot is not null)
            {
                existingSlotByNewSlot.Add(newSlot, leftoverExistingSlot);
                unclaimedExistingSlots.Remove(leftoverExistingSlot);
            }
        }

        return orderedNewSlots.Select(newSlot =>
        {
            var existingSlot = existingSlotByNewSlot.GetValueOrDefault(newSlot);
            if (existingSlot is null)
            {
                return newSlot;
            }

            return new LeagueSlot(existingSlot.LeagueSlotID, newSlot.CounterPick, newSlot.SlotNumber, newSlot.SlotName, newSlot.RequiredTags);
        }).ToList();
    }
}
