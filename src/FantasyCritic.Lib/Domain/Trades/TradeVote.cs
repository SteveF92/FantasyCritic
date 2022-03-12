using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Trades;

public record TradeVote(Guid TradeID, FantasyCriticUser User, bool Approved, Maybe<string> Comment, Instant Timestamp);