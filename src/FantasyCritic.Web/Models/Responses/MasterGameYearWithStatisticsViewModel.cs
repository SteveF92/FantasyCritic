namespace FantasyCritic.Web.Models.Responses;

public class MasterGameYearWithStatisticsViewModel
{
    public MasterGameYearWithStatisticsViewModel(MasterGameYear domain, IEnumerable<MasterGameYearStatistics> statistics, LocalDate currentDate)
    {
        MasterGameYear = new MasterGameYearViewModel(domain, currentDate);
        Statistics = statistics.Select(x => new MasterGameYearStatisticsViewModel(x, domain.Year)).ToList();
    }

    public MasterGameYearViewModel MasterGameYear { get; }
    public IReadOnlyList<MasterGameYearStatisticsViewModel> Statistics { get; }
}
