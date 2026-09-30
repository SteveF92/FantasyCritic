using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Web.Models.Responses;

public class PendingMasterGameUpdatesViewModel
{
    public PendingMasterGameUpdatesViewModel(PendingMasterGameUpdates domain, LocalDate currentDate)
    {
        NewGames = domain.NewGames.Select(x => new PendingNewMasterGameViewModel(x, currentDate)).ToList();
        ScoreUpdates = domain.ScoreUpdates.Select(x => new PendingScoreUpdateViewModel(x, currentDate)).ToList();
        Edits = domain.Edits.Select(x => new PendingMasterGameEditViewModel(x, currentDate)).ToList();
    }

    public IReadOnlyList<PendingNewMasterGameViewModel> NewGames { get; }
    public IReadOnlyList<PendingScoreUpdateViewModel> ScoreUpdates { get; }
    public IReadOnlyList<PendingMasterGameEditViewModel> Edits { get; }
}
