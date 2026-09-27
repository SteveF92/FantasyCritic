using System.Globalization;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Utilities;

//Eastern time throughout: the Saturday evening action processing run is already Sunday in UTC.
public static class DatabaseSnapshotNames
{
    //To the second: only one SnapshotDatabase job can be open at a time, and each waits minutes for AWS, so two can't share a second.
    public static string Admin(Instant time)
    {
        var easternTime = time.InZone(TimeExtensions.EasternTimeZone).LocalDateTime;
        return "admin-snap-" + easternTime.ToString("yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture);
    }

    //Date only. A second run on the same day is a mistake, and AWS rejecting the duplicate name surfaces it.
    public static string PreActionProcessing(Instant time)
    {
        var easternDate = time.InZone(TimeExtensions.EasternTimeZone).Date;
        return "pre-action-processing-snap-" + easternDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
