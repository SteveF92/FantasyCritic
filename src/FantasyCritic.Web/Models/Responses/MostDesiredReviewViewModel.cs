using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Web.Models.Responses;

public class MostDesiredReviewViewModel
{
    public MostDesiredReviewViewModel(MasterGameDesireResult result, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(result.MasterGameYear, currentDate);
        DesireFactor = result.DesireFactor;
    }

    public MasterGameYearViewModel MasterGame { get; }
    public int DesireFactor { get; }
}
