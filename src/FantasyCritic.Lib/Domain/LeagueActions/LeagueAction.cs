using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.Lib.Domain.LeagueActions;

public class LeagueAction
{
    public LeagueAction(Publisher publisher, Instant timestamp, string actionType, string description,
        bool managerAction)
    {
        Publisher = publisher;
        Timestamp = timestamp;
        ActionType = actionType;
        Description = description;
        ManagerAction = managerAction;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(ClaimGameDomainRequest action, Instant timestamp, bool managerAction, bool draft, bool autoDraft)
    {
        Timestamp = timestamp;
        Publisher = action.Publisher;
        if (draft)
        {
            if (!autoDraft)
            {
                if (action.CounterPick)
                {
                    ActionType = "Publisher Counterpick Drafted";
                    Description = $"Drafted game: '{action.GameName}'";
                }
                else
                {
                    ActionType = "Publisher Game Drafted";
                    Description = $"Drafted game: '{action.GameName}'";
                }
            }
            else
            {
                if (action.CounterPick)
                {
                    ActionType = "Publisher Counterpick Auto Drafted";
                    Description = $"Auto Drafted game: '{action.GameName}'";
                }
                else
                {
                    ActionType = "Publisher Game Auto Drafted";
                    Description = $"Auto Drafted game: '{action.GameName}'";
                }
            }
        }
        else
        {
            if (action.CounterPick)
            {
                ActionType = "Publisher Counterpick Claimed";
                Description = $"Claimed game: '{action.GameName}'";
            }
            else
            {
                ActionType = "Publisher Game Claimed";
                Description = $"Claimed game: '{action.GameName}'";
            }
        }

        ManagerAction = managerAction;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(AssociateGameDomainRequest action, Instant timestamp)
    {
        Timestamp = timestamp;
        Publisher = action.Publisher;
        ActionType = "Publisher Game Associated";
        Description = $"Associated publisher game '{action.PublisherGame.GameName}' with master game '{action.MasterGame.GameName}'";
        ManagerAction = true;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(RemoveGameDomainRequest action, Instant timestamp)
    {
        Timestamp = timestamp;
        Publisher = action.Publisher;
        ActionType = "Publisher Game Removed";
        Description = $"Removed game: '{action.PublisherGame.GameName}'";
        ManagerAction = true;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(EditPublisherRequest editValues, Instant timestamp)
    {
        Timestamp = timestamp;
        Publisher = editValues.Publisher;
        ActionType = "Publisher Edited";
        Description = editValues.GetActionString();
        ManagerAction = true;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(PickupBid action, Instant timestamp)
    {
        Timestamp = timestamp;
        Publisher = action.Publisher;

        if (!action.CounterPick)
        {
            ActionType = "Pickup Successful";
            if (action.ConditionalDropPublisherGame is not null)
            {
                if (action.ConditionalDropResult!.Result.IsSuccess)
                {
                    Description = $"Acquired game '{action.MasterGame.GameName}' with a bid of ${action.BidAmount}. Dropped game '{action.ConditionalDropPublisherGame.MasterGame!.MasterGame.GameName}' conditionally.";
                }
                else
                {
                    Description = $"Acquired game '{action.MasterGame.GameName}' with a bid of ${action.BidAmount}. Attempted to drop game '{action.ConditionalDropPublisherGame.MasterGame!.MasterGame.GameName}' conditionally but cannot because: {action.ConditionalDropResult.Result.Error}.";
                }
            }
            else
            {
                Description = $"Acquired game '{action.MasterGame.GameName}' with a bid of ${action.BidAmount}";
            }
        }
        else
        {
            ActionType = "Counter Pick Pickup Successful";
            Description = $"Acquired counter pick '{action.MasterGame.GameName}' with a bid of ${action.BidAmount}";
        }

        ManagerAction = false;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(FailedPickupBid action, Instant timestamp)
    {
        Timestamp = timestamp;
        Publisher = action.PickupBid.Publisher;
        if (!action.PickupBid.CounterPick)
        {
            ActionType = "Pickup Failed";
            Description = $"Tried to acquire game '{action.PickupBid.MasterGame.GameName}' with a bid of ${action.PickupBid.BidAmount}. Failure reason: {action.FailureReason}";
        }
        else
        {
            ActionType = "Counter Pick Pickup Failed";
            Description = $"Tried to acquire counter pick '{action.PickupBid.MasterGame.GameName}' with a bid of ${action.PickupBid.BidAmount}. Failure reason: {action.FailureReason}";
        }
        ManagerAction = false;
        ActionInternalID = Guid.NewGuid();
    }

    public LeagueAction(DropRequest action, DropResult result, Instant timestamp)
    {
        Timestamp = timestamp;
        Publisher = action.Publisher;
        if (result.Result.IsSuccess)
        {
            ActionType = "Drop Successful";
            Description = $"Dropped game '{action.MasterGame.GameName}'";
        }
        else
        {
            ActionType = "Drop Failed";
            Description = $"Tried to drop game '{action.MasterGame.GameName}'. Failure reason: {result.Result.Error}";
        }

        ManagerAction = false;
        ActionInternalID = Guid.NewGuid();
    }

    public Publisher Publisher { get; }
    public Instant Timestamp { get; }
    public string ActionType { get; }
    public string Description { get; }
    public bool ManagerAction { get; }
    public Guid ActionInternalID { get; }

    public bool IsFailed => Description.Contains("Game is no longer eligible");

    public string MasterGameName
    {
        get
        {
            var name = SubstringSearching.GetBetween(Description, "'", "'");
            return name.IsFailure ? "" : name.Value;
        }
    }

    public override string ToString()
    {
        return $"{ActionType}|{Publisher.PublisherName}|{Description}";
    }
}
