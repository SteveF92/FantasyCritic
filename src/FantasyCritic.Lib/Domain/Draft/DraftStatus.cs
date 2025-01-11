namespace FantasyCritic.Lib.Domain.Draft;
public record DraftStatus(DraftPhase DraftPhase, Publisher NextDraftPublisher, Publisher? PreviousDraftPublisher, int DraftPosition, int OverallDraftPosition);
