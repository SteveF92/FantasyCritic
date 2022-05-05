using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;
public class TradeService
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly IClock _clock;

    public TradeService(LeagueMemberService leagueMemberService, InterLeagueService interLeagueService,
        IFantasyCriticRepo fantasyCriticRepo, IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _clock = clock;
    }

    public async Task<Result> ProposeTrade(LeagueYear leagueYear, Publisher proposer, Guid counterPartyPublisherID, IReadOnlyList<Guid> proposerPublisherGameIDs,
        IReadOnlyList<Guid> counterPartyPublisherGameIDs, uint proposerBudgetSendAmount, uint counterPartyBudgetSendAmount, string message)
    {
        if (leagueYear.Options.TradingSystem.Equals(TradingSystem.NoTrades))
        {
            return Result.Failure("Trades are not enabled for this league year.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Failure("Trades must include a message.");
        }

        if (proposerBudgetSendAmount > 0 && counterPartyBudgetSendAmount > 0)
        {
            return Result.Failure("You cannot have budget on both sides of the trade.");
        }

        if (!proposerPublisherGameIDs.Any() && proposerBudgetSendAmount == 0)
        {
            return Result.Failure("You must offer something.");
        }

        if (!counterPartyPublisherGameIDs.Any() && counterPartyBudgetSendAmount == 0)
        {
            return Result.Failure("You must receive something.");
        }

        var counterPartyResult = leagueYear.GetPublisherByID(counterPartyPublisherID);
        if (counterPartyResult is null)
        {
            return Result.Failure("That publisher does not exist");
        }
        var counterParty = counterPartyResult;

        if (!leagueYear.Key.Equals(counterParty.LeagueYearKey))
        {
            return Result.Failure("That publisher is not in your league.");
        }

        if (proposerBudgetSendAmount > proposer.Budget)
        {
            return Result.Failure("You do not have enough budget for this trade.");
        }

        if (counterPartyBudgetSendAmount > counterParty.Budget)
        {
            return Result.Failure("The other publisher does not have enough budget for this trade.");
        }

        var proposerPublisherGames = proposer.PublisherGames.Where(x => proposerPublisherGameIDs.Contains(x.PublisherGameID)).ToList();
        var counterPartyPublisherGames = counterParty.PublisherGames.Where(x => counterPartyPublisherGameIDs.Contains(x.PublisherGameID)).ToList();
        if (proposerPublisherGames.Count != proposerPublisherGameIDs.Count)
        {
            return Result.Failure("Some of the proposer publisher games are invalid or duplicates.");
        }

        if (counterPartyPublisherGames.Count != counterPartyPublisherGameIDs.Count)
        {
            return Result.Failure("Some of the counter party publisher games are invalid or duplicates.");
        }

        var proposerPublisherGamesWithMasterGames = proposerPublisherGames.Select(x => x.GetMasterGameYearWithCounterPick()).Where(x => x is not null).Select(x => x!).ToList();
        var counterPartyPublisherGamesWithMasterGames = counterPartyPublisherGames.Select(x => x.GetMasterGameYearWithCounterPick()).Where(x => x is not null).Select(x => x!).ToList();
        if (proposerPublisherGamesWithMasterGames.Count != proposerPublisherGameIDs.Count)
        {
            return Result.Failure("All games in a trade must be linked to a master game.");
        }

        if (counterPartyPublisherGamesWithMasterGames.Count != counterPartyPublisherGameIDs.Count)
        {
            return Result.Failure("All games in a trade must be linked to a master game.");
        }

        Trade trade = new Trade(Guid.NewGuid(), leagueYear, proposer, counterParty, proposerPublisherGamesWithMasterGames,
            counterPartyPublisherGamesWithMasterGames,
            proposerBudgetSendAmount, counterPartyBudgetSendAmount, message, _clock.GetCurrentInstant(), null, null,
            new List<TradeVote>(), TradeStatus.Proposed);

        var tradeError = trade.GetTradeError();
        if (tradeError is not null)
        {
            return Result.Failure(tradeError);
        }

        await _fantasyCriticRepo.CreateTrade(trade);

        return Result.Success();
    }

    public Task<Trade?> GetTrade(Guid tradeID)
    {
        return _fantasyCriticRepo.GetTrade(tradeID);
    }

    public async Task<IReadOnlyList<Trade>> GetTradesForLeague(LeagueYear leagueYear)
    {
        var trades = await _fantasyCriticRepo.GetTradesForLeague(leagueYear);
        return trades;
    }

    public async Task<IReadOnlyList<Trade>> GetActiveTradesForLeague(LeagueYear leagueYear)
    {
        var allTrades = await GetTradesForLeague(leagueYear);
        var activeTrades = allTrades.Where(x => x.Status.IsActive).ToList();
        return activeTrades;
    }

    public async Task<Result> RescindTrade(Trade trade)
    {
        if (!trade.Status.IsActive)
        {
            return Result.Failure("That trade cannot be rescinded as it is no longer active.");
        }

        var now = _clock.GetCurrentInstant();
        await _fantasyCriticRepo.EditTradeStatus(trade, TradeStatus.Rescinded, null, now);
        return Result.Success();
    }

    public async Task<Result> AcceptTrade(Trade trade)
    {
        if (!trade.Status.IsActive)
        {
            return Result.Failure("That trade cannot be accepted as it is no longer active.");
        }

        var now = _clock.GetCurrentInstant();
        await _fantasyCriticRepo.EditTradeStatus(trade, TradeStatus.Accepted, now, null);
        return Result.Success();
    }

    public async Task<Result> RejectTradeByCounterParty(Trade trade)
    {
        if (!trade.Status.IsActive)
        {
            return Result.Failure("That trade cannot be rejected as it is no longer active.");
        }

        var now = _clock.GetCurrentInstant();
        await _fantasyCriticRepo.EditTradeStatus(trade, TradeStatus.RejectedByCounterParty, null, now);
        return Result.Success();
    }

    public async Task<Result> RejectTradeByManager(Trade trade)
    {
        if (!trade.Status.IsActive)
        {
            return Result.Failure("That trade cannot be rejected as it is no longer active.");
        }

        var now = _clock.GetCurrentInstant();
        await _fantasyCriticRepo.EditTradeStatus(trade, TradeStatus.RejectedByManager, null, now);
        return Result.Success();
    }

    public async Task<Result> VoteOnTrade(Trade trade, FantasyCriticUser user, bool approved, string? comment)
    {
        var alreadyVoted = trade.TradeVotes.Select(x => x.User.Id).ToHashSet().Contains(user.Id);
        if (alreadyVoted)
        {
            return Result.Failure("You have already vote on this trade.");
        }

        var tradeVote = new TradeVote(trade.TradeID, user, approved, comment, _clock.GetCurrentInstant());
        await _fantasyCriticRepo.AddTradeVote(tradeVote);
        return Result.Success();
    }

    public async Task<Result> DeleteTradeVote(Trade trade, FantasyCriticUser user)
    {
        var alreadyVoted = trade.TradeVotes.Select(x => x.User.Id).ToHashSet().Contains(user.Id);
        if (!alreadyVoted)
        {
            return Result.Failure("You have note voted on this trade.");
        }

        await _fantasyCriticRepo.DeleteTradeVote(trade, user);
        return Result.Success();
    }

    public async Task<Result> ExecuteTrade(Trade trade)
    {
        if (!trade.Status.Equals(TradeStatus.Accepted))
        {
            return Result.Failure("Only accepted trades can be executed.");
        }

        var tradeError = trade.GetTradeError();
        if (tradeError is not null)
        {
            return Result.Failure(tradeError);
        }

        var completionTime = _clock.GetCurrentInstant();
        var newPublisherGamesResult = trade.GetNewPublisherGamesFromTrade(completionTime);
        if (newPublisherGamesResult.IsFailure)
        {
            return Result.Failure(newPublisherGamesResult.Error);
        }

        var executedTrade = new ExecutedTrade(trade, completionTime, newPublisherGamesResult.Value);
        await _fantasyCriticRepo.ExecuteTrade(executedTrade);
        return Result.Success();
    }
}
