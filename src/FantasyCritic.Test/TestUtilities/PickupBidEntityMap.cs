using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.Lib.SharedSerialization.Database;

namespace FantasyCritic.Test.TestUtilities;
public sealed class PreAllowIneligibleSlotPickupBidEntityMap : ClassMap<PickupBidEntity>
{
    public PreAllowIneligibleSlotPickupBidEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.AllowIneligibleSlot).Constant(true);
    }
}

public sealed class PickupBidEntityMap : ClassMap<PickupBidEntity>
{
    public PickupBidEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
    }
}
