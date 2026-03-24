using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class SupportUserSearchMatchViewModel
{
    public SupportUserSearchMatchViewModel(FantasyCriticUser user, IReadOnlyList<LeaguePublisherRowForUser> leagueRows)
    {
        User = new FantasyCriticUserViewModel(user);
        Leagues = leagueRows.Select(x => new SupportTicketLeagueLinkViewModel(x)).ToList();
    }

    public FantasyCriticUserViewModel User { get; }
    public IReadOnlyList<SupportTicketLeagueLinkViewModel> Leagues { get; }
}
