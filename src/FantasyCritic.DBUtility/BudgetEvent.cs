using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Trades;
using NodaTime;

namespace FantasyCritic.DBUtility;
public class BudgetEvent
{
    public BudgetEvent(long previousBudget, LeagueAction editPublisherAction)
    {
        PreviousBudget = previousBudget;
        Timestamp = editPublisherAction.Timestamp;
        var parts = editPublisherAction.Description.Split(" ");
        var lastPart = parts.Last().TrimEnd('.');
        NewBudget = long.Parse(lastPart);
        Change = PreviousBudget - NewBudget;
        Description = editPublisherAction.Description;
    }

    public BudgetEvent(long previousBudget, Trade trade, bool isProposer)
    {
        PreviousBudget = previousBudget;
        Timestamp = trade.CompletedTimestamp!.Value;

        string partyString = "";
        if (isProposer)
        {
            partyString = "(Prop)";
            Change += trade.ProposerBudgetSendAmount * -1;
            Change += trade.CounterPartyBudgetSendAmount;
        }
        else
        {
            partyString = "(Counter)";
            Change += trade.CounterPartyBudgetSendAmount * -1;
            Change += trade.ProposerBudgetSendAmount;
        }

        NewBudget = PreviousBudget + Change;
        if (Change > 0)
        {
            Description = $"{partyString} | Acquired ${Change} in trade.";
        }
        else
        {
            Description = $"{partyString} | Traded away ${Change * -1}.";
        }
    }

    public BudgetEvent(long previousBudget, PickupBid bid)
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
