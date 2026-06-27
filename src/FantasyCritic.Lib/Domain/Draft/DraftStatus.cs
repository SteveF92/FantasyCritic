namespace FantasyCritic.Lib.Domain.Draft;

public class DraftStatus
{
    public DraftStatus(LeagueDraft draft, FutureDraftPick nextPick, PastDraftPick? previousPick, PastDraftPick? previousNonSkippedPick, IReadOnlyList<FutureDraftPick> picksToSkip)
    {
        Draft = draft;
        NextPick = nextPick;
        PreviousPick = previousPick;
        PreviousNonSkippedPick = previousNonSkippedPick;
        PicksToSkip = picksToSkip;
    }

    public LeagueDraft Draft { get; }
    public FutureDraftPick NextPick { get; }
    public PastDraftPick? PreviousPick { get; }
    public PastDraftPick? PreviousNonSkippedPick { get; }
    public IReadOnlyList<FutureDraftPick> PicksToSkip { get; }

    public DraftPhase DraftPhase
    {
        get
        {
            if (NextPick.CounterPick)
            {
                return DraftPhase.CounterPicks;
            }

            return DraftPhase.StandardGames;
        }
    }

    public Publisher NextDraftPublisher => NextPick.Publisher;
    public Publisher? PreviousPublisherThatWasNotSkipped => PreviousNonSkippedPick?.Publisher;
    public int RoundNumber => NextPick.RoundNumber;
    public int OverallPickNumber => NextPick.OverallPickNumber;
}

public record PastDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, int OverallPickNumber, PublisherGame? GameSelected)
{
    public bool Skipped => GameSelected is null;
}

public record FutureDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, int OverallPickNumber, bool WillBeSkipped);

public record PickProcessingResult(FutureDraftPick? NextPick, IReadOnlyList<FutureDraftPick> PicksToSkip);
