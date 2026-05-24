namespace FantasyCritic.Web.Models.Responses;

public class LongestTenuredGameViewModel
{
    public LongestTenuredGameViewModel(LongestTenuredGame result, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(result.MasterGameYear, currentDate);
        DreamsDashed = result.DreamsDashed;
        DreamsRealized = result.DreamsRealized;
        YearOfPeakHype = result.YearOfPeakHype;
        YearOfPeakHypeCount = result.YearOfPeakHypeCount;
    }

    public MasterGameYearViewModel MasterGame { get; }
    public int DreamsDashed { get; }
    public int? DreamsRealized { get; }
    public int? YearOfPeakHype { get; }
    public int? YearOfPeakHypeCount { get; }
}
