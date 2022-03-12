namespace FantasyCritic.Lib.Domain;

public class SpecialGameSlot : IEquatable<SpecialGameSlot>
{
    public SpecialGameSlot(int specialSlotPosition, IEnumerable<MasterGameTag> tags)
    {
        SpecialSlotPosition = specialSlotPosition;
        Tags = tags.ToList();
    }

    public int SpecialSlotPosition { get; }
    public IReadOnlyList<MasterGameTag> Tags { get; }

    public override string ToString() => $"{SpecialSlotPosition}: {string.Join("|", Tags.Select(x => x.ReadableName))}";

    public bool Equals(SpecialGameSlot other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return SpecialSlotPosition == other.SpecialSlotPosition && Tags.OrderBy(x => x.Name).SequenceEqual(other.Tags.OrderBy(x => x.Name));
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SpecialGameSlot)obj);
    }

    public override int GetHashCode()
    {
        int listHash = 19;
        unchecked
        {
            foreach (var tag in Tags)
            {
                listHash = listHash * 31 + tag.GetHashCode();
            }
        }
        return HashCode.Combine(SpecialSlotPosition, listHash);
    }
}
