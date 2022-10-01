using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.SharedSerialization.Database;

namespace FantasyCritic.Test.TestUtilities;
public sealed class MasterGameYearEntityMap : ClassMap<MasterGameYearEntity>
{
    public MasterGameYearEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.OpenCriticSlug).Constant(null);
    }
}
