namespace FantasyCritic.Web.Models.Responses;
public class CompleteMasterGameChangeViewModel
{
    public CompleteMasterGameChangeViewModel(MasterGameChangeLogEntry change, LocalDate currentDate)
    {
        MasterGame = new MasterGameViewModel(change.MasterGame, currentDate);
        Change = new MasterGameChangeLogEntryViewModel(change);
    }

    public MasterGameViewModel MasterGame { get; }
    public MasterGameChangeLogEntryViewModel Change { get; }
}
