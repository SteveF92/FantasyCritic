using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Royale;

public record RoyaleGroupMemberWithLifetimeStats(VeryMinimalFantasyCriticUser User, int QuartersParticipated, decimal TotalPoints, double? AverageRankWithinGroup);
