using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain.Trades;

public class Trade
{
    public Trade(Guid tradeID, LeagueYear leagueYear, Publisher proposer, Publisher counterParty, IEnumerable<MasterGameYearWithCounterPick> proposerMasterGames,
        IEnumerable<MasterGameYearWithCounterPick> counterPartyMasterGames, uint proposerBudgetSendAmount, uint counterPartyBudgetSendAmount,
        string message, Instant proposedTimestamp, Instant? acceptedTimestamp, Instant? completedTimestamp,
        IEnumerable<TradeVote> tradeVotes, TradeStatus status)
    {
        TradeID = tradeID;
        LeagueYear = leagueYear;
        Proposer = proposer;
        CounterParty = counterParty;
        ProposerMasterGames = proposerMasterGames.ToList();
        CounterPartyMasterGames = counterPartyMasterGames.ToList();
        ProposerBudgetSendAmount = proposerBudgetSendAmount;
        CounterPartyBudgetSendAmount = counterPartyBudgetSendAmount;
        Message = message;
        ProposedTimestamp = proposedTimestamp;
        AcceptedTimestamp = acceptedTimestamp;
        CompletedTimestamp = completedTimestamp;
        TradeVotes = tradeVotes.ToList();
        Status = status;
    }

    public Guid TradeID { get; }
    public LeagueYear LeagueYear { get; }
    public Publisher Proposer { get; }
    public Publisher CounterParty { get; }
    public IReadOnlyList<MasterGameYearWithCounterPick> ProposerMasterGames { get; }
    public IReadOnlyList<MasterGameYearWithCounterPick> CounterPartyMasterGames { get; }
    public uint ProposerBudgetSendAmount { get; }
    public uint CounterPartyBudgetSendAmount { get; }
    public string Message { get; }
    public Instant ProposedTimestamp { get; }
    public Instant? AcceptedTimestamp { get; }
    public Instant? CompletedTimestamp { get; }
    public IReadOnlyList<TradeVote> TradeVotes { get; }
    public TradeStatus Status { get; }

    public IReadOnlyList<Guid> GetPublisherIDs()
    {
        return new List<Guid>()
        {
            Proposer.PublisherID,
            CounterParty.PublisherID
        };
    }

    public Instant? GetExpirationTime()
    {
        if (!Status.IsActive)
        {
            return null;
        }

        List<Instant> actionTimestamps = new List<Instant>()
        {
            ProposedTimestamp
        };

        if (AcceptedTimestamp.HasValue)
        {
            actionTimestamps.Add(AcceptedTimestamp.Value);
        }

        actionTimestamps.AddRange(TradeVotes.Select(x => x.Timestamp));

        var maxTime = actionTimestamps.Max();
        return maxTime.Plus(Duration.FromDays(7));
    }

    public string? GetTradeError()
    {
        if (Proposer.PublisherID == Guid.Empty || CounterParty.PublisherID == Guid.Empty)
        {
            return "One of the publishers involved in this trade no longer exists.";
        }

        if (LeagueYear.Options.TradingSystem.Equals(TradingSystem.NoTrades))
        {
            return "Trades are not enabled for this league year.";
        }

        if (ProposerBudgetSendAmount > Proposer.Budget)
        {
            return $"{Proposer.PublisherName} does not have enough budget for this trade.";
        }

        if (CounterPartyBudgetSendAmount > CounterParty.Budget)
        {
            return $"{CounterParty.PublisherName} does not have enough budget for this trade.";
        }

        var proposerPublisherGamesWithMasterGames = Proposer.PublisherGames.Select(x => x.GetMasterGameYearWithCounterPick()).Where(x => x is not null).Select(x => x).ToList();
        var counterPartyPublisherGamesWithMasterGames = CounterParty.PublisherGames.Select(x => x.GetMasterGameYearWithCounterPick()).Where(x => x is not null).Select(x => x).ToList();

        bool proposerGamesValid = proposerPublisherGamesWithMasterGames.ContainsAllItems(ProposerMasterGames);
        bool counterPartyGamesValid = counterPartyPublisherGamesWithMasterGames.ContainsAllItems(CounterPartyMasterGames);
        if (!proposerGamesValid)
        {
            return $"{Proposer.PublisherName} no longer has all of the games involved in this trade.";
        }

        if (!counterPartyGamesValid)
        {
            return $"{CounterParty.PublisherName} no longer has all of the games involved in this trade.";
        }

        var totalNumberStandardGameSlotsForLeague = LeagueYear.Options.StandardGames;
        var totalNumberCounterPickSlotsForLeague = LeagueYear.Options.CounterPicks;

        var resultingProposerStandardGames = GetResultingGameCount(Proposer, ProposerMasterGames, CounterPartyMasterGames, false);
        var resultingCounterPartyStandardGames = GetResultingGameCount(CounterParty, CounterPartyMasterGames, ProposerMasterGames, false);
        var resultingProposerCounterPickGames = GetResultingGameCount(Proposer, ProposerMasterGames, CounterPartyMasterGames, true);
        var resultingCounterPartyCounterPickGames = GetResultingGameCount(CounterParty, CounterPartyMasterGames, ProposerMasterGames, true);

        if (resultingProposerStandardGames > totalNumberStandardGameSlotsForLeague)
        {
            return $"{Proposer.PublisherName} does not have enough standard slots available to complete this trade.";
        }
        if (resultingCounterPartyStandardGames > totalNumberStandardGameSlotsForLeague)
        {
            return $"{CounterParty.PublisherName} does not have enough standard slots available to complete this trade.";
        }
        if (resultingProposerCounterPickGames > totalNumberCounterPickSlotsForLeague)
        {
            return $"{Proposer.PublisherName} does not have enough counter pick slots available to complete this trade.";
        }
        if (resultingCounterPartyCounterPickGames > totalNumberCounterPickSlotsForLeague)
        {
            return $"{CounterParty.PublisherName} does not have enough counter pick slots available to complete this trade.";
        }

        return null;
    }

    private static int GetResultingGameCount(Publisher publisher, IEnumerable<MasterGameYearWithCounterPick> gamesTradingAway, IEnumerable<MasterGameYearWithCounterPick> gamesAcquiring, bool counterPick)
    {
        var currentGamesCount = publisher.PublisherGames.Count(x => x.CounterPick == counterPick);
        var gamesRemovedCount = gamesTradingAway.Count(x => x.CounterPick == counterPick);
        var gamesAcquiredCount = gamesAcquiring.Count(x => x.CounterPick == counterPick);
        var resultingNumberOfGames = currentGamesCount - gamesRemovedCount + gamesAcquiredCount;
        return resultingNumberOfGames;
    }

    public IReadOnlyList<Publisher> GetUpdatedPublishers(IEnumerable<PublisherGame> newPublisherGames)
    {
        var publisherStateSet = new PublisherStateSet(new List<Publisher>()
        {
            Proposer,
            CounterParty
        });

        publisherStateSet.ObtainBudgetForPublisher(Proposer, CounterPartyBudgetSendAmount);
        publisherStateSet.SpendBudgetForPublisher(Proposer, ProposerBudgetSendAmount);
        publisherStateSet.ObtainBudgetForPublisher(CounterParty, ProposerBudgetSendAmount);
        publisherStateSet.SpendBudgetForPublisher(CounterParty, CounterPartyBudgetSendAmount);

        var newProposerPublisherGames = newPublisherGames.Where(x => x.PublisherID == Proposer.PublisherID).ToList();
        var newCounterPartyPublisherGames = newPublisherGames.Where(x => x.PublisherID == CounterParty.PublisherID).ToList();

        publisherStateSet.UpdateGamesForPublisher(Proposer, ProposerMasterGames, newProposerPublisherGames);
        publisherStateSet.UpdateGamesForPublisher(CounterParty, CounterPartyMasterGames, newCounterPartyPublisherGames);

        return publisherStateSet.Publishers;
    }

    public IReadOnlyList<LeagueAction> GetActions(Instant actionTime)
    {
        var proposerAction = GetActionForPublisher(Proposer, actionTime, CounterPartyMasterGames, CounterPartyBudgetSendAmount);
        var counterPartyAction = GetActionForPublisher(CounterParty, actionTime, ProposerMasterGames, ProposerBudgetSendAmount);
        return new List<LeagueAction>()
        {
            proposerAction,
            counterPartyAction
        };
    }

    private static LeagueAction GetActionForPublisher(Publisher publisher, Instant actionTime, IEnumerable<MasterGameYearWithCounterPick> games, uint budgetSend)
    {
        List<string> acquisitions = new List<string>();
        foreach (var game in games)
        {
            var counterPickString = "";
            if (game.CounterPick)
            {
                counterPickString = " (Counter Pick)";
            }
            acquisitions.Add($"Acquired '{game.MasterGameYear.MasterGame.GameName}'{counterPickString}.");
        }
        if (budgetSend > 0)
        {
            acquisitions.Add($"Acquired ${budgetSend} of budget.");
        }

        string finalString = string.Join("\n", acquisitions.Select(x => $"• {x}"));
        var proposerAction = new LeagueAction(publisher, actionTime, "Trade Executed", finalString, true);
        return proposerAction;
    }

    public IReadOnlyList<FormerPublisherGame> GetRemovedPublisherGames(Instant completionTime)
    {
        List<FormerPublisherGame> formerGames = new List<FormerPublisherGame>();
        foreach (var proposerGame in Proposer.PublisherGames)
        {
            var masterGameWithCounterPick = proposerGame.GetMasterGameYearWithCounterPick();
            if (masterGameWithCounterPick is null)
            {
                continue;
            }
            if (ProposerMasterGames.Contains(masterGameWithCounterPick))
            {
                formerGames.Add(proposerGame.GetFormerPublisherGame(completionTime, $"Traded to {CounterParty.PublisherName}"));
            }
        }
        foreach (var counterPartyGame in CounterParty.PublisherGames)
        {
            var masterGameWithCounterPick = counterPartyGame.GetMasterGameYearWithCounterPick();
            if (masterGameWithCounterPick is null)
            {
                continue;
            }
            if (CounterPartyMasterGames.Contains(masterGameWithCounterPick))
            {
                formerGames.Add(counterPartyGame.GetFormerPublisherGame(completionTime, $"Traded to {Proposer.PublisherName}"));
            }
        }

        return formerGames;
    }

    public Result<IReadOnlyList<PublisherGame>> GetNewPublisherGamesFromTrade(Instant completionTime)
    {
        var dateOfAcquisition = completionTime.ToEasternDate();
        var proposerGameDictionary = Proposer.PublisherGames.Where(x => x.MasterGame is not null).ToDictionary(x => x.GetMasterGameYearWithCounterPick()!);
        var counterPartyGameDictionary = CounterParty.PublisherGames.Where(x => x.MasterGame is not null).ToDictionary(x => x.GetMasterGameYearWithCounterPick()!);

        List<PotentialPublisherSlot> newlyOpenProposerSlotNumbers = new List<PotentialPublisherSlot>();
        foreach (var game in ProposerMasterGames)
        {
            var existingPublisherGame = proposerGameDictionary[game];
            newlyOpenProposerSlotNumbers.Add(new PotentialPublisherSlot(existingPublisherGame.SlotNumber, game.CounterPick));
        }
        List<PotentialPublisherSlot> newlyOpenCounterPartySlotNumbers = new List<PotentialPublisherSlot>();
        foreach (var game in CounterPartyMasterGames)
        {
            var existingPublisherGame = counterPartyGameDictionary[game];
            newlyOpenCounterPartySlotNumbers.Add(new PotentialPublisherSlot(existingPublisherGame.SlotNumber, game.CounterPick));
        }

        var alreadyOpenProposerSlotNumbers = Proposer.GetPublisherSlots(LeagueYear).Where(x => x.PublisherGame is null).Select(x => new PotentialPublisherSlot(x.SlotNumber, x.CounterPick));
        var alreadyOpenCounterPartySlotNumbers = CounterParty.GetPublisherSlots(LeagueYear).Where(x => x.PublisherGame is null).Select(x => new PotentialPublisherSlot(x.SlotNumber, x.CounterPick));
        var allOpenProposerSlotNumbers = alreadyOpenProposerSlotNumbers.Concat(newlyOpenProposerSlotNumbers).Distinct().OrderBy(x => x.CounterPick).ThenBy(x => x.SlotNumber).ToList();
        var allOpenCounterPartySlotNumbers = alreadyOpenCounterPartySlotNumbers.Concat(newlyOpenCounterPartySlotNumbers).Distinct().OrderBy(x => x.CounterPick).ThenBy(x => x.SlotNumber).ToList();

        List<PublisherGame> newPublisherGames = new List<PublisherGame>();
        foreach (var game in ProposerMasterGames)
        {
            var existingPublisherGame = proposerGameDictionary[game];
            var eligibilityFactors = LeagueYear.GetEligibilityFactorsForMasterGame(game.MasterGameYear.MasterGame, dateOfAcquisition);
            var openSlotNumbers = allOpenCounterPartySlotNumbers.Where(x => x.CounterPick == game.CounterPick).Select(x => x.SlotNumber);
            var slotResult = SlotEligibilityFunctions.GetTradeSlotResult(CounterParty, LeagueYear, game, eligibilityFactors, openSlotNumbers);
            if (!slotResult.HasValue)
            {
                return Result.Failure<IReadOnlyList<PublisherGame>>($"Cannot find an appropriate slot for: {game.MasterGameYear.MasterGame.GameName}");
            }

            allOpenCounterPartySlotNumbers = allOpenCounterPartySlotNumbers.Where(x => !(x.SlotNumber == slotResult.Value && x.CounterPick == game.CounterPick)).ToList();
            PublisherGame newPublisherGame = new PublisherGame(CounterParty.PublisherID, Guid.NewGuid(), game.MasterGameYear.MasterGame.GameName, completionTime,
                game.CounterPick, existingPublisherGame.ManualCriticScore, existingPublisherGame.ManualWillNotRelease, existingPublisherGame.FantasyPoints, game.MasterGameYear, slotResult.Value, null, null, null, TradeID);
            newPublisherGames.Add(newPublisherGame);
        }
        foreach (var game in CounterPartyMasterGames)
        {
            var existingPublisherGame = counterPartyGameDictionary[game];
            var eligibilityFactors = LeagueYear.GetEligibilityFactorsForMasterGame(game.MasterGameYear.MasterGame, dateOfAcquisition);
            var openSlotNumbers = allOpenProposerSlotNumbers.Where(x => x.CounterPick == game.CounterPick).Select(x => x.SlotNumber);
            var slotResult = SlotEligibilityFunctions.GetTradeSlotResult(Proposer, LeagueYear, game, eligibilityFactors, openSlotNumbers);
            if (!slotResult.HasValue)
            {
                return Result.Failure<IReadOnlyList<PublisherGame>>($"Cannot find an appropriate slot for: {game.MasterGameYear.MasterGame.GameName}");
            }

            allOpenProposerSlotNumbers = allOpenProposerSlotNumbers.Where(x => !(x.SlotNumber == slotResult.Value && x.CounterPick == game.CounterPick)).ToList();
            PublisherGame newPublisherGame = new PublisherGame(Proposer.PublisherID, Guid.NewGuid(), game.MasterGameYear.MasterGame.GameName, completionTime,
                game.CounterPick, existingPublisherGame.ManualCriticScore, existingPublisherGame.ManualWillNotRelease, existingPublisherGame.FantasyPoints, game.MasterGameYear, slotResult.Value, null, null, null, TradeID);
            newPublisherGames.Add(newPublisherGame);
        }

        return newPublisherGames;
    }

    public Trade UpdateTrade(TradeStatus status, Instant? acceptedTimestamp, Instant? completedTimestamp)
    {
        return new Trade(TradeID, LeagueYear, Proposer, CounterParty, ProposerMasterGames, CounterPartyMasterGames,
            ProposerBudgetSendAmount, CounterPartyBudgetSendAmount, Message, ProposedTimestamp, acceptedTimestamp, completedTimestamp, TradeVotes, status);
    }
}
