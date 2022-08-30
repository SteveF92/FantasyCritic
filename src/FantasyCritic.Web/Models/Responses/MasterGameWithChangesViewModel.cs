namespace FantasyCritic.Web.Models.Responses;

public class MasterGameWithChangesViewModel
{
    public MasterGameWithChangesViewModel(MasterGame masterGame, LocalDate currentDate,
        int numberOutstandingCorrections, IReadOnlyList<MasterGameChangeLogEntry> changes)
    {
        MasterGame = new MasterGameViewModel(masterGame, currentDate, false, numberOutstandingCorrections);
        Changes = changes.Select(x => new MasterGameChangeLogEntryViewModel(x)).ToList();
    }

    public MasterGameViewModel MasterGame { get; }
    public IReadOnlyList<MasterGameChangeLogEntryViewModel> Changes { get; }
}
