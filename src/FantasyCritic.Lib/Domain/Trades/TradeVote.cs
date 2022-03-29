using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Trades;

public record TradeVote(Guid TradeID, FantasyCriticUser User, bool Approved, string? Comment, Instant Timestamp);
