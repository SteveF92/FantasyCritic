namespace FantasyCritic.Lib.Domain.Combinations;

public record MasterGameYearWithStatistics(MasterGameYear MasterGameYear, IReadOnlyList<MasterGameYearStatistics> Statistics);
