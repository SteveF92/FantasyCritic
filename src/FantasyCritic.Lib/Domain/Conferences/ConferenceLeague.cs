using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceLeague : IEquatable<ConferenceLeague>
{
    public ConferenceLeague(Guid leagueID, string leagueName, MinimalFantasyCriticUser leagueManager)
    {
        LeagueID = leagueID;
        LeagueName = leagueName;
        LeagueManager = leagueManager;
    }

    public Guid LeagueID { get; }
    public string LeagueName { get; }
    public MinimalFantasyCriticUser LeagueManager { get; }

    public override string ToString() => LeagueName;

    public bool Equals(ConferenceLeague? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return LeagueID.Equals(other.LeagueID);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ConferenceLeague) obj);
    }

    public override int GetHashCode()
    {
        return LeagueID.GetHashCode();
    }
}
