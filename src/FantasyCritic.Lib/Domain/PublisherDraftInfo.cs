namespace FantasyCritic.Lib.Domain;

public class PublisherDraftInfo
{
    public PublisherDraftInfo(Guid draftID, Guid publisherID, int draftPosition)
    {
        DraftID = draftID;
        PublisherID = publisherID;
        DraftPosition = draftPosition;
    }

    public Guid DraftID { get; }
    public Guid PublisherID { get; }
    public int DraftPosition { get; }
}
