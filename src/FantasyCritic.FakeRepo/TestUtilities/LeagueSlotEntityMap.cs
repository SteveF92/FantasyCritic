using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.FakeRepo.TestUtilities;

internal sealed class LeagueSlotEntityMap : ClassMap<LeagueSlotEntity>
{
    public LeagueSlotEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.SlotName).TypeConverterOption.NullValues(string.Empty);
    }
}
