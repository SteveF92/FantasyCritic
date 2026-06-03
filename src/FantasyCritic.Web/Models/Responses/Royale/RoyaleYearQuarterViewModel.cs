using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleYearQuarterViewModel
{
    public RoyaleYearQuarterViewModel(RoyaleYearQuarter domain)
    {
        Year = domain.YearQuarter.Year;
        Quarter = domain.YearQuarter.Quarter;
        OpenForPlay = domain.OpenForPlay;
        Finished = domain.Finished;
    }

    public int Year { get; }
    public int Quarter { get; }
    public bool OpenForPlay { get; }
    public bool Finished { get; }
}
