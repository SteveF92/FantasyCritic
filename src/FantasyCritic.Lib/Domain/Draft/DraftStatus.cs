namespace FantasyCritic.Lib.Domain.Draft;
public record DraftStatus(DraftPhase DraftPhase, Publisher NextDraftPublisher, int DraftPosition, int OverallDraftPosition);
