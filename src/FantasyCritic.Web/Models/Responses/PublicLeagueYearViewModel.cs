namespace FantasyCritic.Web.Models.Responses;

public class PublicLeagueYearViewModel
{
    public PublicLeagueYearViewModel(PublicLeagueYearStats leagueYear)
    {
        LeagueID = leagueYear.LeagueID;
        LeagueName = leagueYear.LeagueName;
        NumberOfFollowers = leagueYear.NumberOfFollowers;
        PlayStatus = leagueYear.PlayStatus.Value;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int NumberOfFollowers { get; }
    public string PlayStatus { get; }
}
