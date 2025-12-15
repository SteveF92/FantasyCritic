namespace FantasyCritic.Lib.Royale;

public record RoyaleAction(RoyalePublisher Publisher, RoyalePublisherGame PublisherGame, string ActionType, string Description, Instant Timestamp);
