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
    public int OverallPickNumber => NextPick.OverallPickNumber ?? throw new InvalidOperationException($"NextPick for publisher {NextPick.Publisher.PublisherID} has no pick number. NextPick should never be a skipped pick.");
}

public record PastDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, PublisherGame? GameSelected)
{
    public bool Skipped => GameSelected is null;
    public int? OverallPickNumber => GameSelected?.OverallDraftPosition;
}

public record FutureDraftPick(Publisher Publisher, bool CounterPick, int RoundNumber, int? OverallPickNumber)
{
    public bool WillBeSkipped => OverallPickNumber is null;
}

public record PickProcessingResult(FutureDraftPick? NextPick, IReadOnlyList<FutureDraftPick> PicksToSkip);
