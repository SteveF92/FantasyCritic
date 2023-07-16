using FantasyCritic.Lib.Domain;

namespace FantasyCritic.EmailTemplates.Models;

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
