using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses;

public class SupportTicketLeagueLinkViewModel
{
    public SupportTicketLeagueLinkViewModel(LeaguePublisherRowForUser row)
    {
        LeagueID = row.LeagueID;
        LeagueName = row.LeagueName;
        Year = row.Year;
        PublisherName = row.PublisherName;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public string PublisherName { get; }
}
