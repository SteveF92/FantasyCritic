using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public record RoyaleGroupMemberDisplayRow(VeryMinimalFantasyCriticUser User, RoyalePublisher? Publisher, IReadOnlyList<RoyalePublisherStatistics> Statistics);
public record RoyaleGroupWithMemberDisplayRows(RoyaleGroup RoyaleGroup, IReadOnlyList<RoyaleGroupMemberDisplayRow> DisplayRows);
