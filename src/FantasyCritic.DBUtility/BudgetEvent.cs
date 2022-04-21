using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using NodaTime;

namespace FantasyCritic.DBUtility;
public class BudgetEvent
{
    public BudgetEvent(uint previousBudget, LeagueAction editPublisherAction)
    {
        PreviousBudget = previousBudget;
        Timestamp = editPublisherAction.Timestamp;
        var parts = editPublisherAction.Description.Split(" ");
        var lastPart = parts.Last().TrimEnd('.');
        NewBudget = long.Parse(lastPart);
        Change = PreviousBudget - NewBudget;
        Description = editPublisherAction.Description;
    }

    public BudgetEvent(uint previousBudget, Trade trade, bool isProposer)
    {
        PreviousBudget = previousBudget;
        Timestamp = trade.CompletedTimestamp!.Value;

        if (isProposer)
        {
            Change += trade.ProposerBudgetSendAmount * -1;
            Change += trade.CounterPartyBudgetSendAmount;
        }
        else
        {
            Change += trade.CounterPartyBudgetSendAmount * -1;
            Change += trade.ProposerBudgetSendAmount;
        }

        NewBudget = PreviousBudget + Change;
        if (Change > 0)
        {
            Description = $"Acquired ${Change} in trade.";
        }
        else
        {
            Description = $"Traded away ${Change}.";
        }
    }

    public BudgetEvent(uint previousBudget, PickupBid bid)
    {
        PreviousBudget = previousBudget;
        Timestamp = bid.Timestamp;

        Change = bid.BidAmount * -1;
        NewBudget = PreviousBudget + Change;
        Description = $"Won {bid.MasterGame.GameName} with bid of {bid.BidAmount}";
    }

    public Instant Timestamp { get; }
    public long PreviousBudget { get; }
    public long Change { get; }
    public long NewBudget { get; }
    public string Description { get; }
}
