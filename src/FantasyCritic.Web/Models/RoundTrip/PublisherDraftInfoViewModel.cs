namespace FantasyCritic.Web.Models.RoundTrip;

public class PublisherDraftInfoViewModel
{
    public PublisherDraftInfoViewModel()
    {

    }

    public PublisherDraftInfoViewModel(PublisherDraftInfo domain)
    {
        DraftID = domain.DraftID;
        DraftNumber = domain.DraftNumber;
        PublisherID = domain.PublisherID;
        DraftPosition = domain.DraftPosition;
    }

    public Guid DraftID { get; set; }
    public int DraftNumber { get; set; }
    public Guid PublisherID { get; set; }
    public int DraftPosition { get; set; }

    public PublisherDraftInfo ToDomain()
    {
        return new PublisherDraftInfo(DraftID, DraftNumber, PublisherID, DraftPosition, new List<PublisherDraftPickSkip>());
    }
}
