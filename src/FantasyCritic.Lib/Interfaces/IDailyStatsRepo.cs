using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces;

public interface IDailyStatsRepo
{
    Task UpdateDailyStats(IEnumerable<SupportedYear> activeYears, IEnumerable<RoyaleYearQuarter> royaleQuarters, LocalDate currentDate, SystemWideValues systemWideValues);
}
