namespace FantasyCritic.Lib.Domain.ScoringSystems;

public abstract class ScoringSystem : IEquatable<ScoringSystem>
{
    public static ScoringSystem GetScoringSystem(string scoringSystemName)
    {
        if (scoringSystemName == "Standard")
        {
            return new StandardScoringSystem();
        }

        if (scoringSystemName == "Diminishing")
        {
            return new DiminishingScoringSystem();
        }

        throw new Exception($"Scoring system not implemented: {scoringSystemName}");
    }

    public static ScoringSystem GetDefaultScoringSystem(int year)
    {
        if (year < 2021)
        {
            return new StandardScoringSystem();
        }

        return new DiminishingScoringSystem();
    }

    public static IReadOnlyList<ScoringSystem> GetAllPossibleValues()
    {
        return new List<ScoringSystem>() { new StandardScoringSystem(), new DiminishingScoringSystem() };
    }

    public abstract string Name { get; }

    public abstract decimal GetPointsForScore(decimal criticScore, bool counterPick);

    public bool Equals(ScoringSystem other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ScoringSystem)obj);
    }

    public override int GetHashCode()
    {
        return (Name != null ? Name.GetHashCode() : 0);
    }

    public override string ToString() => Name;
}
