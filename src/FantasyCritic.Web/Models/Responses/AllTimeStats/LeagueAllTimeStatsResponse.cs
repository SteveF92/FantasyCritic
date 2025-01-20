using FantasyCritic.Lib.Domain.AllTimeStats;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class LeagueAllTimeStatsResponse
{
    public LeagueAllTimeStatsResponse(LeagueViewModel leagueViewModel, LeagueAllTimeStats allTimeStats, SystemWideValues systemWideValues, LocalDate currentDate)
    {
        League = leagueViewModel;

        PlayerAllTimeStats = allTimeStats.PlayerAllTimeStats.Select(x => new LeaguePlayerAllTimeStatsResponse(x)).ToList();
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

        HallOfFameGameLists = allTimeStats.HallOfFameGameLists.Select(x => new HallOfFameGameListResponse(x, currentDate)).ToList();
    }

    public LeagueViewModel League { get; }
    public List<LeaguePlayerAllTimeStatsResponse> PlayerAllTimeStats { get; }
    public List<AllTimeStatsPublisherViewModel> Publishers { get; }
    public List<HallOfFameGameListResponse> HallOfFameGameLists { get; }
}
