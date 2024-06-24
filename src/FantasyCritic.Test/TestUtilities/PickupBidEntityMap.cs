using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.Lib.SharedSerialization.Database;
using NodaTime.Text;

namespace FantasyCritic.Test.TestUtilities;
public sealed class PreAllowIneligibleSlotPickupBidEntityMap : ClassMap<PickupBidEntity>
{
    private static readonly InstantPattern pattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");
    public PreAllowIneligibleSlotPickupBidEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.AllowIneligibleSlot).Constant(true);
        Map(m => m.Timestamp).Convert(args => pattern.Parse(args.Row.GetField("Timestamp")!).GetValueOrThrow());
    }
}

public sealed class PickupBidEntityMap : ClassMap<PickupBidEntity>
{
    private static readonly InstantPattern pattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");
    public PickupBidEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Timestamp).Convert(args => pattern.Parse(args.Row.GetField("Timestamp")!).GetValueOrThrow());
    }
}
