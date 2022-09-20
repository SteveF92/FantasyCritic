using FantasyCritic.Lib.Domain;

namespace FantasyCritic.SharedSerialization;

public class SpecialGameSlotEntity
{
    public SpecialGameSlotEntity()
    {

    }

    public SpecialGameSlotEntity(Guid specialSlotID, League league, int year, int specialSlotPosition, MasterGameTag tag)
    {
        SpecialSlotID = specialSlotID;
        LeagueID = league.LeagueID;
        Year = year;
        SpecialSlotPosition = specialSlotPosition;
        Tag = tag.Name;
    }

    public Guid SpecialSlotID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int SpecialSlotPosition { get; set; }
    public string Tag { get; set; } = null!;

    public static ILookup<LeagueYearKey, SpecialGameSlot> ConvertSpecialGameSlotEntities(IEnumerable<SpecialGameSlotEntity> specialGameSlotEntities, IReadOnlyDictionary<string, MasterGameTag> tagOptions)
    {
        Dictionary<LeagueYearKey, List<SpecialGameSlot>> fullDomains = new Dictionary<LeagueYearKey, List<SpecialGameSlot>>();
        var groupByLeagueYearKey = specialGameSlotEntities.GroupBy(x => new LeagueYearKey(x.LeagueID, x.Year));
        foreach (var leagueYearGroup in groupByLeagueYearKey)
        {
            List<SpecialGameSlot> domainsForLeagueYear = new List<SpecialGameSlot>();
            var groupByPosition = leagueYearGroup.GroupBy(x => x.SpecialSlotPosition);
            foreach (var positionGroup in groupByPosition)
            {
                var tags = positionGroup.Select(x => tagOptions[x.Tag]);
                domainsForLeagueYear.Add(new SpecialGameSlot(positionGroup.Key, tags));
            }

            fullDomains[leagueYearGroup.Key] = domainsForLeagueYear;
        }

        return fullDomains.SelectMany(p => p.Value.Select(x => new { p.Key, Value = x })).ToLookup(pair => pair.Key, pair => pair.Value);
    }
}
