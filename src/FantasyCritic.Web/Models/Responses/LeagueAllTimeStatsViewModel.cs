namespace FantasyCritic.Web.Models.Responses;

public class LeagueAllTimeStatsViewModel
{
    public LeagueAllTimeStatsViewModel(LeagueViewModel leagueViewModel)
    {
        League = leagueViewModel;
    }

    public LeagueViewModel League { get; }
}
