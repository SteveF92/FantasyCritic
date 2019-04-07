using System;

namespace FantasyCritic.Lib.Scheduling.Lib.Cron
{
    [Serializable]
    public enum CrontabFieldKind
    {
        Minute,
        Hour,
        Day,
        Month,
        DayOfWeek
    }
}