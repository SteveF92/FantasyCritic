namespace FantasyCritic.Lib.Domain;

public interface IPublisherGame
{
    Guid PublisherID { get; }
    Guid PublisherGameID { get; }
    string OriginalGameName { get; }
    Instant Timestamp { get; }
    bool CounterPick { get; }
    decimal? ManualCriticScore { get; }
    bool ManualWillNotRelease { get; }
    decimal? FantasyPoints { get; }
    MasterGameYear? MasterGame { get; }
    int SlotNumber { get; }
    int? DraftPosition { get; }
    int? OverallDraftPosition { get; }
    Guid? DraftID { get; }
    uint? BidAmount { get; }
    Guid? AcquiredInTradeID { get; }
    string GameName { get; }
    bool IsFormerPublisherGame { get; }
}
