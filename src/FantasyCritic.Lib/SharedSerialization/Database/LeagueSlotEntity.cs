namespace FantasyCritic.Lib.SharedSerialization.Database;

public class LeagueSlotEntity
{
    public LeagueSlotEntity()
    {

    }

    public LeagueSlotEntity(LeagueYearKey leagueYearKey, LeagueSlot domain)
    {
        LeagueSlotID = domain.LeagueSlotID;
        LeagueID = leagueYearKey.LeagueID;
        Year = leagueYearKey.Year;
        CounterPick = domain.CounterPick;
        SlotNumber = domain.SlotNumber;
        SlotName = domain.SlotName;
    }

    public Guid LeagueSlotID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public bool CounterPick { get; set; }
    public int SlotNumber { get; set; }
    public string? SlotName { get; set; }

    public LeagueSlot ToDomain(IEnumerable<MasterGameTag> requiredTags)
    {
        return new LeagueSlot(LeagueSlotID, CounterPick, SlotNumber, SlotName, requiredTags);
    }

    public static ILookup<LeagueYearKey, LeagueSlot> ConvertLeagueSlotEntities(IEnumerable<LeagueSlotEntity> leagueSlotEntities, IEnumerable<LeagueSlotRequiresTagEntity> leagueSlotRequiresTagEntities, IReadOnlyDictionary<string, MasterGameTag> tagOptions)
    {
        Dictionary<LeagueYearKey, List<LeagueSlot>> fullDomains = [];
        var groupByLeagueYearKey = leagueSlotEntities.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
        var tagRequirementsLookup = leagueSlotRequiresTagEntities.ToLookup(x => x.LeagueSlotID);
        foreach (var leagueYearGroup in groupByLeagueYearKey)
        {
            var domainsForLeagueYear = leagueYearGroup.Select(x => x.ToDomain(tagRequirementsLookup[x.LeagueSlotID].Select(y => tagOptions[y.Tag]))).ToList();
            fullDomains[leagueYearGroup.Key] = domainsForLeagueYear;
        }

        return fullDomains.SelectMany(p => p.Value.Select(x => new { p.Key, Value = x })).ToLookup(pair => pair.Key, pair => pair.Value);
    }
}
