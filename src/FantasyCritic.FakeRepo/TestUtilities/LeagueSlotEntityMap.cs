using System.Globalization;
using CsvHelper.Configuration;

namespace FantasyCritic.FakeRepo.TestUtilities;

internal sealed class LeagueSlotEntityMap : ClassMap<LeagueSlotEntity>
{
    public LeagueSlotEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.SlotName).TypeConverterOption.NullValues(string.Empty);
    }
}
