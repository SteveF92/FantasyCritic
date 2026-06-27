namespace FantasyCritic.Lib.Domain.Draft;

public class DraftStatus
{
    private DraftStatus(LeagueDraft draft, IReadOnlyList<DraftTurn> draftTurns, DraftPhase draftPhase, Publisher nextDraftPublisher, Publisher? previousDraftPublisher, int draftPosition, int overallDraftPosition)
    {
        Draft = draft;
        DraftTurns = draftTurns;
        DraftPhase = draftPhase;
        NextDraftPublisher = nextDraftPublisher;
        PreviousDraftPublisher = previousDraftPublisher;
        DraftPosition = draftPosition;
        OverallDraftPosition = overallDraftPosition;
    }

    public LeagueDraft Draft { get; }
    public IReadOnlyList<DraftTurn> DraftTurns { get; }
    public DraftPhase DraftPhase { get; }
    public Publisher NextDraftPublisher { get; }
    public Publisher? PreviousDraftPublisher { get; }
    public int DraftPosition { get; }
    public int OverallDraftPosition { get; }

    public static DraftStatus? BuildDraftStatus(LeagueDraft draft, IReadOnlyList<DraftTurn> draftTurns)
    {
        var nextTurn = draftTurns.FirstOrDefault(x => x.Skipped is null);
        if (nextTurn is null)
        {
            return null;
        }

        var previousTurn = draftTurns.LastOrDefault(x => x.Skipped == false);
        return new DraftStatus(draft, draftTurns, nextTurn.DraftPhase, nextTurn.Publisher, previousTurn?.Publisher, nextTurn.DraftPosition, nextTurn.OverallDraftPosition);
    }
}

public record DraftTurn(DraftPhase DraftPhase, Publisher Publisher, int DraftPosition, int OverallDraftPosition, PublisherGame? GameSelected, bool? Skipped);
