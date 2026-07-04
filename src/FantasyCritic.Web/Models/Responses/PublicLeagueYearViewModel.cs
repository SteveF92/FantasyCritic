namespace FantasyCritic.Web.Models.Responses;

public class PublicLeagueYearViewModel
{
    public PublicLeagueYearViewModel(PublicLeagueYearStats leagueYear)
    {
        LeagueID = leagueYear.LeagueID;
        LeagueName = leagueYear.LeagueName;
        NumberOfFollowers = leagueYear.NumberOfFollowers;
        AnyDraftStarted = leagueYear.AnyDraftStarted;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public int NumberOfFollowers { get; }
    public bool AnyDraftStarted { get; }
}
