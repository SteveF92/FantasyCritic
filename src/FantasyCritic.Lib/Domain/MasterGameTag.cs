namespace FantasyCritic.Lib.Domain;

public class MasterGameTag : IEquatable<MasterGameTag>
{
    public MasterGameTag(string name, string readableName, string shortName, MasterGameTagType tagType,
        bool hasCustomCode, bool systemTagOnly, string description, IEnumerable<string> examples, string badgeColor)
    {
        Name = name;
        ReadableName = readableName;
        ShortName = shortName;
        TagType = tagType;
        HasCustomCode = hasCustomCode;
        SystemTagOnly = systemTagOnly;
        Description = description;
        Examples = examples.ToList();
        BadgeColor = badgeColor;
    }

    public string Name { get; }
    public string ReadableName { get; }
    public string ShortName { get; }
    public MasterGameTagType TagType { get; }
    public bool HasCustomCode { get; }
    public bool SystemTagOnly { get; }
    public string Description { get; }
    public IReadOnlyList<string> Examples { get; }
    public string BadgeColor { get; }

    public bool Equals(MasterGameTag? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MasterGameTag)obj);
    }

    public override int GetHashCode()
    {
        return (Name != null ? Name.GetHashCode() : 0);
    }

    public override string ToString() => Name;
}
