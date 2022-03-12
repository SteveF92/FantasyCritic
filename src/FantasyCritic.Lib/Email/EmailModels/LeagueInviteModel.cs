namespace FantasyCritic.Lib.Email.EmailModels;

public class LeagueInviteModel
{
    public LeagueInviteModel(League league, string baseURL)
    {
        League = league;
        Link = baseURL;
    }

    public League League { get; }
    public string Link { get; }
}
