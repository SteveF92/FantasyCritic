using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearStandingViewModel
{
    public ConferenceYearStandingViewModel(ConferenceYearStanding domain)
    {
        LeagueID = domain.LeagueID;
        Year = domain.Year;
        PublisherID = domain.PublisherID;
        DisplayName = domain.DisplayName;
        PublisherName = domain.PublisherName;
        FantasyPoints = domain.FantasyPoints;
        ProjectedPoints = domain.ProjectedPoints;
    }
    
    public Guid LeagueID { get; }
    public int Year { get; }
    public Guid PublisherID { get; }
    public string DisplayName { get; }
    public string PublisherName { get; }
    public decimal FantasyPoints { get; }
    public decimal ProjectedPoints { get; }
}
