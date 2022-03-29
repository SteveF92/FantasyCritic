using FantasyCritic.Lib.Domain.Trades;

namespace FantasyCritic.MySQL.Entities.Trades;

public class TradeVoteEntity
{
    public TradeVoteEntity()
    {

    }

    public TradeVoteEntity(TradeVote domain)
    {
        TradeID = domain.TradeID;
        UserID = domain.User.Id;
        Approved = domain.Approved;
        Comment = domain.Comment;
        Timestamp = domain.Timestamp;
    }

    public Guid TradeID { get; set; }
    public Guid UserID { get; set; }
    public bool Approved { get; set; }
    public string? Comment { get; set; }
    public Instant Timestamp { get; set; }
}
