using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces;

public interface IDailyStatsRepo
{
    public Task UpdateDailyStats(IEnumerable<SupportedYear> activeYears, IEnumerable<RoyaleYearQuarter> royaleQuarters, LocalDate currentDate, SystemWideValues systemWideValues);
}
