namespace FantasyCritic.Lib.Domain.Draft;
public record DraftStatus(LeagueDraft Draft, DraftPhase DraftPhase, Publisher NextDraftPublisher, Publisher? PreviousDraftPublisher, int DraftPosition, int OverallDraftPosition, IReadOnlyList<DraftTurn> DraftTurns);
public record DraftTurn(DraftPhase DraftPhase, Publisher Publisher, int DraftPosition, int OverallDraftPosition, PublisherGame? GameSelected, bool? Skipped);
