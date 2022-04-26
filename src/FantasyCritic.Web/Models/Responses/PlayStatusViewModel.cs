using FantasyCritic.Lib.Domain.Draft;

namespace FantasyCritic.Web.Models.Responses;

public class PlayStatusViewModel
{
    public PlayStatusViewModel(CompletePlayStatus completePlayStatus)
    {
        PlayStatus = completePlayStatus.PlayStatus.Value;
        ReadyToSetDraftOrder = completePlayStatus.ReadyToSetDraftOrder;
        ReadyToDraft = completePlayStatus.ReadyToDraft;
        PlayStarted = completePlayStatus.PlayStatus.PlayStarted;
        DraftIsActive = completePlayStatus.PlayStatus.DraftIsActive;
        DraftIsPaused = completePlayStatus.PlayStatus.DraftIsPaused;
        DraftFinished = completePlayStatus.PlayStatus.DraftFinished;
        StartDraftErrors = completePlayStatus.StartDraftErrors;

        if (DraftPhase.CounterPicks.Equals(completePlayStatus.DraftStatus?.DraftPhase))
        {
            DraftingCounterPicks = true;
        }
    }

    public string PlayStatus { get; }
    public bool ReadyToSetDraftOrder { get; }
    public bool ReadyToDraft { get; }
    public bool PlayStarted { get; }
    public bool DraftIsActive { get; }
    public bool DraftIsPaused { get; }
    public bool DraftFinished { get; }
    public bool DraftingCounterPicks { get; }
    public IReadOnlyList<string> StartDraftErrors { get; }
}
