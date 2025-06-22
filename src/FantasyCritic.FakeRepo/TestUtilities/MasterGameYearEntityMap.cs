using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.FakeRepo.TestUtilities;
public sealed class MasterGameYearEntityMap : ClassMap<MasterGameYearEntity>
{
    public MasterGameYearEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.OpenCriticSlug).Constant(null);
        Map(m => m.GGSlug).Constant(null);
        Map(m => m.ShowNote).Constant(false);
        Map(m => m.AddedByUserDisplayName).Constant("elitefusion");
    }
}
