namespace FantasyCritic.MySQL.Entities;

internal class LeagueDraftPublisherEntity
{
    public LeagueDraftPublisherEntity()
    {

    }

    public LeagueDraftPublisherEntity(LeagueDraft draft, Guid publisherID, int draftPosition)
    {
        LeagueID = draft.LeagueYearKey.LeagueID;
        Year = draft.LeagueYearKey.Year;
        DraftID = draft.DraftID;
        PublisherID = publisherID;
        DraftPosition = draftPosition;
    }

    public LeagueDraftPublisherEntity(LeagueYearKey leagueYearKey, Guid draftID, Guid publisherID, int draftPosition)
    {
        LeagueID = leagueYearKey.LeagueID;
        Year = leagueYearKey.Year;
        DraftID = draftID;
        PublisherID = publisherID;
        DraftPosition = draftPosition;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid DraftID { get; set; }
    public Guid PublisherID { get; set; }
    public int DraftPosition { get; set; }
}
