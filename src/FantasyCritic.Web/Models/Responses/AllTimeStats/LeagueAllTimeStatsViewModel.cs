namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class LeagueAllTimeStatsViewModel
{
    public LeagueAllTimeStatsViewModel(LeagueViewModel leagueViewModel, LeagueAllTimeStats allTimeStats, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        League = leagueViewModel;

        PlayerAllTimeStats = allTimeStats.PlayerAllTimeStats.Select(x => new LeaguePlayerAllTimeStatsViewModel(x)).ToList();
        Publishers = new List<AllTimeStatsPublisherViewModel>();
        foreach (var leagueYear in allTimeStats.LeagueYears)
        {
            foreach (var publisher in leagueYear.Publishers)
            {
                var ranking = leagueYear.Publishers.Count(y =>
                    y.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options) >
                    publisher.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options)) + 1;
                Publishers.Add(new AllTimeStatsPublisherViewModel(leagueYear, publisher, ranking, systemWideValues, currentDate));
            }
        }
    }

    public LeagueViewModel League { get; }
    public List<LeaguePlayerAllTimeStatsViewModel> PlayerAllTimeStats { get; }
    public List<AllTimeStatsPublisherViewModel> Publishers { get; }
}
