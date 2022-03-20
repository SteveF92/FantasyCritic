using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Helpers;
public class LeagueUserRelationship
{
    public LeagueUserRelationship(bool inOrInvitedToLeague, bool leagueManager)
    {
        InOrInvitedToLeague = inOrInvitedToLeague;
        LeagueManager = leagueManager;
    }

    public bool InOrInvitedToLeague { get; }
    public bool LeagueManager { get; }
}

public class LeagueYearUserRelationship
{
    public LeagueYearUserRelationship(bool inOrInvitedToLeague, bool activeInYear, bool leagueManager)
    {
        InOrInvitedToLeague = inOrInvitedToLeague;
        ActiveInYear = activeInYear;
        LeagueManager = leagueManager;
    }

    public bool InOrInvitedToLeague { get; }
    public bool ActiveInYear { get; }
    public bool LeagueManager { get; }
}

[Flags]
public enum RequiredRelationshipLevel
{
    MustBeLoggedIn,
    MustBeInLeague,
    MustBeActiveInYear,
    MustBe
};

public enum LeagueStatus
{
    AllowAnonymous,
    MustBeLoggedIn
};
