namespace FantasyCritic.Lib.Domain.Draft;

public class DraftStatus
{
    public DraftStatus(LeagueDraft draft, DraftTurn nextTurn, DraftTurn? previousTurn, IReadOnlyList<DraftTurn> turnsToSkip)
    {
        Draft = draft;
        NextTurn = nextTurn;
        PreviousTurn = previousTurn;
        TurnsToSkip = turnsToSkip;
    }

    public LeagueDraft Draft { get; }
    public DraftTurn NextTurn { get; }
    public DraftTurn? PreviousTurn { get; }
    public IReadOnlyList<DraftTurn> TurnsToSkip { get; }

    public DraftPhase DraftPhase => NextTurn.DraftPhase;
    public Publisher NextDraftPublisher => NextTurn.Publisher;
    public Publisher? PreviousDraftPublisher => PreviousTurn?.Publisher;
    public int DraftPosition => NextTurn.DraftPosition;
    public int OverallDraftPosition => NextTurn.OverallDraftPosition;
}

public record DraftTurn(DraftPhase DraftPhase, Publisher Publisher, int DraftPosition, int OverallDraftPosition, PublisherGame? GameSelected, bool? Skipped);
public record TurnProcessingResult(DraftTurn? NextTurn, DraftTurn? PreviousTurn, IReadOnlyList<DraftTurn> TurnsToSkip);
