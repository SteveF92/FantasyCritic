using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public class Conference : IEquatable<Conference>
{
    public Conference(Guid conferenceID, string conferenceName, MinimalFantasyCriticUser conferenceManager, IEnumerable<int> years, bool customRulesConference, Guid primaryLeagueID, IEnumerable<Guid> leaguesInConference)
    {
        ConferenceID = conferenceID;
        ConferenceName = conferenceName;
        ConferenceManager = conferenceManager;
        Years = years.ToList();
        CustomRulesConference = customRulesConference;
        PrimaryLeagueID = primaryLeagueID;
        LeaguesInConference = leaguesInConference.ToList();
    }

    public Guid ConferenceID { get; }
    public string ConferenceName { get; }
    public MinimalFantasyCriticUser ConferenceManager { get; }
    public IReadOnlyList<int> Years { get; }
    public bool CustomRulesConference { get; }
    public Guid PrimaryLeagueID { get; }
    public IReadOnlyList<Guid> LeaguesInConference { get; }

    public bool Equals(Conference? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ConferenceID.Equals(other.ConferenceID);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Conference)obj);
    }

    public override int GetHashCode()
    {
        return ConferenceID.GetHashCode();
    }

    public override string ToString() => $"{ConferenceID}|{ConferenceName}";
}
