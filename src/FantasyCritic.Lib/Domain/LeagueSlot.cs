namespace FantasyCritic.Lib.Domain;

public class LeagueSlot : IEquatable<LeagueSlot>
{
    public LeagueSlot(Guid leagueSlotID, bool counterPick, int slotNumber, string? slotName,
        IEnumerable<MasterGameTag> requiredTags)
    {
        LeagueSlotID = leagueSlotID;
        CounterPick = counterPick;
        SlotNumber = slotNumber;
        SlotName = slotName;
        RequiredTags = requiredTags.ToList();
    }

    public Guid LeagueSlotID { get; }
    public bool CounterPick { get; }
    public int SlotNumber { get; }
    public string? SlotName { get; }
    public IReadOnlyList<MasterGameTag> RequiredTags { get; }

    public bool IsSpecial => RequiredTags.Any();

    public int GetOverallSlotNumber(LeagueOptions leagueOptions)
    {
        if (!CounterPick)
        {
            return SlotNumber;
        }

        return leagueOptions.StandardGames + SlotNumber;
    }

    public int? GetSpecialSlotPosition(LeagueOptions leagueOptions)
    {
        if (!IsSpecial)
        {
            return null;
        }

        return leagueOptions.SpecialGameSlots.Count(x => x.SlotNumber < SlotNumber);
    }

    public override string ToString()
    {
        var cpk = "";
        if (CounterPick)
        {
            cpk = "CPK-";
        }
        var slotType = "REG";
        if (RequiredTags.Any())
        {
            if (RequiredTags.Count > 1)
            {
                slotType = "FLX";
            }
            else
            {
                slotType = RequiredTags[0].ShortName;
            }
        }

        List<string> stringParts = [$"{cpk}{SlotNumber}", slotType];
        if (SlotName is not null)
        {
            stringParts.Add(SlotName);
        }
        return string.Join('|', stringParts);
    }

    public bool Equals(LeagueSlot? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return LeagueSlotID.Equals(other.LeagueSlotID);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((LeagueSlot)obj);
    }

    public override int GetHashCode()
    {
        return LeagueSlotID.GetHashCode();
    }
}
