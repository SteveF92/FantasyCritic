namespace FantasyCritic.Lib.Domain;

public class PublisherDraftInfo
{
    public PublisherDraftInfo(Guid draftID, int draftNumber, Guid publisherID, int draftPosition, IReadOnlyList<PublisherDraftPickSkip> pickSkips)
    {
        DraftID = draftID;
        DraftNumber = draftNumber;
        PublisherID = publisherID;
        DraftPosition = draftPosition;
        PickSkips = pickSkips;
    }

    public Guid DraftID { get; }
    public int DraftNumber { get; }
    public Guid PublisherID { get; }
    public int DraftPosition { get; }
    public IReadOnlyList<PublisherDraftPickSkip> PickSkips { get; }
}

public record PublisherDraftPickSkip(bool CounterPick, int PickNumber, bool IsManualSkip);
