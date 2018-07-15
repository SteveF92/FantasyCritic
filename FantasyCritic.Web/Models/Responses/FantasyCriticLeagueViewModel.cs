using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class FantasyCriticLeagueViewModel
    {
        public FantasyCriticLeagueViewModel(FantasyCriticLeague league)
        {
            LeagueName = league.LeagueName;
            LeagueManager = new FantasyCriticPlayerViewModel(league.LeagueManager);
        }

        public string LeagueName { get; }
        public FantasyCriticPlayerViewModel LeagueManager { get; }
    }
}
