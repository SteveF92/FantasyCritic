namespace FantasyCritic.MySQL.Entities;

internal class SetDraftOrderEntity
{
    public SetDraftOrderEntity(Guid publisherID, int draftPosition)
    {
        PublisherID = publisherID;
        DraftPosition = draftPosition;
    }

    public Guid PublisherID { get; }
    public int DraftPosition { get; }
}