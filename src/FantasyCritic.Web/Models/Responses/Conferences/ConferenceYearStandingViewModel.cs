using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearStandingViewModel
{
    public ConferenceYearStandingViewModel(ConferenceYearStanding domain, int ranking, int projectedRanking, int leagueRanking, int projectedLeagueRanking)
    {
        LeagueID = domain.LeagueID;
        LeagueName = domain.LeagueName;
        Year = domain.Year;
        PublisherID = domain.PublisherID;
        DisplayName = domain.DisplayName;
        PublisherName = domain.PublisherName;
        TotalFantasyPoints = domain.TotalFantasyPoints;
        ProjectedFantasyPoints = domain.ProjectedFantasyPoints;
        Ranking = ranking;
        ProjectedRanking = projectedRanking;
        LeagueRanking = leagueRanking;
        ProjectedLeagueRanking = projectedLeagueRanking;
    }
    
    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int Year { get; }
    public Guid PublisherID { get; }
    public string DisplayName { get; }
    public string PublisherName { get; }
    public decimal TotalFantasyPoints { get; }
    public decimal ProjectedFantasyPoints { get; }
    public int Ranking { get; }
    public int ProjectedRanking { get; }
    public int LeagueRanking { get; }
    public int ProjectedLeagueRanking { get; }
}
