using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Domain.Combinations;

public record DraftResult(ClaimResult Result, AutoDraftResult AuthDraftResult, bool DraftComplete);
