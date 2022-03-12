using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;

public class League : IEquatable<League>
{
    public League(Guid leagueID, string leagueName, FantasyCriticUser leagueManager, IEnumerable<int> years, bool publicLeague, bool testLeague, bool archived, int numberOfFollowers)
    {
        LeagueID = leagueID;
        LeagueName = leagueName;
        LeagueManager = leagueManager;
        Years = years.ToList();
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        Archived = archived;
        NumberOfFollowers = numberOfFollowers;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public FantasyCriticUser LeagueManager { get; }
    public IReadOnlyList<int> Years { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool Archived { get; }
    public int NumberOfFollowers { get; }

    public bool Equals(League other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return LeagueID.Equals(other.LeagueID);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((League)obj);
    }

    public override int GetHashCode()
    {
        return LeagueID.GetHashCode();
    }

    public override string ToString() => $"{LeagueID}|{LeagueName}";
}