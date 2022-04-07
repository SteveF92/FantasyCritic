using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class CreateLeagueRequest
{
    public CreateLeagueRequest(int initialYear, string leagueName, int standardGames, int gamesToDraft, int counterPicks,
        int counterPicksToDraft, int freeDroppableGames, int willNotReleaseDroppableGames, int willReleaseDroppableGames, bool unlimitedFreeDroppableGames,
        bool unlimitedWillNotReleaseDroppableGames, bool unlimitedWillReleaseDroppableGames, bool dropOnlyDraftGames, bool counterPicksBlockDrops,
        int minimumBidAmount, string draftSystem, string pickupSystem, string tradingSystem, string tiebreakSystem,
        LocalDate counterPickDeadline, LeagueTagOptionsViewModel tags, List<SpecialGameSlotViewModel> specialGameSlots)
    {
        InitialYear = initialYear;
        LeagueName = leagueName;
        StandardGames = standardGames;
        GamesToDraft = gamesToDraft;
        CounterPicks = counterPicks;
        CounterPicksToDraft = counterPicksToDraft;
        FreeDroppableGames = freeDroppableGames;
        WillNotReleaseDroppableGames = willNotReleaseDroppableGames;
        WillReleaseDroppableGames = willReleaseDroppableGames;
        UnlimitedFreeDroppableGames = unlimitedFreeDroppableGames;
        UnlimitedWillNotReleaseDroppableGames = unlimitedWillNotReleaseDroppableGames;
        UnlimitedWillReleaseDroppableGames = unlimitedWillReleaseDroppableGames;
        DropOnlyDraftGames = dropOnlyDraftGames;
        CounterPicksBlockDrops = counterPicksBlockDrops;
        MinimumBidAmount = minimumBidAmount;
        DraftSystem = draftSystem;
        PickupSystem = pickupSystem;
        TradingSystem = tradingSystem;
        TiebreakSystem = tiebreakSystem;
        CounterPickDeadline = counterPickDeadline;
        Tags = tags;
        SpecialGameSlots = specialGameSlots;
    }

    public int InitialYear { get; }
    public string LeagueName { get; }

    [Range(1, 50)]
    public int StandardGames { get; }
    [Range(1, 50)]
    public int GamesToDraft { get; }
    [Range(0, 5)]
    public int CounterPicks { get; }
    [Range(0, 5)]
    public int CounterPicksToDraft { get; }

    [Range(0, 100)]
    public int FreeDroppableGames { get; }
    [Range(0, 100)]
    public int WillNotReleaseDroppableGames { get; }
    [Range(0, 100)]
    public int WillReleaseDroppableGames { get; }
    public bool UnlimitedFreeDroppableGames { get; }
    public bool UnlimitedWillNotReleaseDroppableGames { get; }
    public bool UnlimitedWillReleaseDroppableGames { get; }
    public bool DropOnlyDraftGames { get; }
    public bool CounterPicksBlockDrops { get; }

    public int MinimumBidAmount { get; }
    public string DraftSystem { get; }
    public string PickupSystem { get; }
    public string TiebreakSystem { get; }
    public string TradingSystem { get; }
    public LocalDate CounterPickDeadline { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public LeagueTagOptionsViewModel Tags { get; }
    public List<SpecialGameSlotViewModel> SpecialGameSlots { get; }

    public Result IsValid()
    {
        if (string.IsNullOrWhiteSpace(LeagueName))
        {
            return Result.Failure("You cannot have a blank league name.");
        }

        if (Tags.Required.Any())
        {
            return Result.Failure("Impressive API usage, but required tags are not ready for prime time yet.");
        }

        if (CounterPickDeadline.Year != InitialYear)
        {
            return Result.Failure($"The counter pick deadline must be in {InitialYear}");
        }

        return Result.Success();
    }

    public LeagueCreationParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        DraftSystem draftSystem = Lib.Enums.DraftSystem.FromValue(DraftSystem);
        PickupSystem pickupSystem = Lib.Enums.PickupSystem.FromValue(PickupSystem);
        TradingSystem tradingSystem = Lib.Enums.TradingSystem.FromValue(TradingSystem);
        TiebreakSystem tiebreakSystem = Lib.Enums.TiebreakSystem.FromValue(TiebreakSystem);
        ScoringSystem scoringSystem = new DiminishingScoringSystem();

        int freeDroppableGames = FreeDroppableGames;
        if (UnlimitedFreeDroppableGames)
        {
            freeDroppableGames = -1;
        }
        int willNotReleaseDroppableGames = WillNotReleaseDroppableGames;
        if (UnlimitedWillNotReleaseDroppableGames)
        {
            willNotReleaseDroppableGames = -1;
        }
        int willReleaseDroppableGames = WillReleaseDroppableGames;
        if (UnlimitedWillReleaseDroppableGames)
        {
            willReleaseDroppableGames = -1;
        }

        var leagueTags = Tags.ToDomain(tagDictionary);
        var specialGameSlots = SpecialGameSlots.Select(x => x.ToDomain(tagDictionary));

        var counterPickDeadline = new AnnualDate(CounterPickDeadline.Month, CounterPickDeadline.Day);

        LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, StandardGames, GamesToDraft, CounterPicks, CounterPicksToDraft,
            freeDroppableGames, willNotReleaseDroppableGames, willReleaseDroppableGames, DropOnlyDraftGames, CounterPicksBlockDrops, MinimumBidAmount,
            InitialYear, leagueTags, specialGameSlots, draftSystem, pickupSystem, tiebreakSystem, scoringSystem, tradingSystem, counterPickDeadline, PublicLeague, TestLeague);
        return parameters;
    }
}
