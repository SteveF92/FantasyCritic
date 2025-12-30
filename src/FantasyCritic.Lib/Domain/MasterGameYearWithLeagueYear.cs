namespace FantasyCritic.Lib.Domain;

public class MasterGameYearWithLeagueYear
{
    public MasterGameYear MasterGameYear { get; init; }
    public LeagueYear LeagueYear { get; init; }

    public MasterGameYearWithLeagueYear(MasterGameYear masterGameYear, LeagueYear leagueYear)
    {
        MasterGameYear = masterGameYear;
        LeagueYear = leagueYear;
    }
}
