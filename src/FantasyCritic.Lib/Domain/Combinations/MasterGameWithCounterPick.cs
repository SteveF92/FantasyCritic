namespace FantasyCritic.Lib.Domain.Combinations;

public record MasterGameWithCounterPickAndActionProcessingSet(MasterGame MasterGame, bool CounterPick, Guid ProcessSetID);
public record MasterGameYearWithCounterPick(MasterGameYear MasterGameYear, bool CounterPick);
