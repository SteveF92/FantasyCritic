namespace FantasyCritic.Web.Models.Responses;

public class MasterGameYearWithCounterPickViewModel
{
    public MasterGameYearWithCounterPickViewModel(MasterGameYear masterGameYear, bool counterPick, LocalDate currentDate)
    {
        MasterGameYear = new MasterGameYearViewModel(masterGameYear, currentDate);
        CounterPick = counterPick;
    }

    public MasterGameYearViewModel MasterGameYear { get; }
    public bool CounterPick { get; }
}