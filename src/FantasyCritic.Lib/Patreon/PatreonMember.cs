namespace FantasyCritic.Lib.Patreon;

/// <summary>
/// A campaign member, reduced to what decides Plus and Donor: their Patreon user, and the titles of the tiers they are
/// currently entitled to.
/// </summary>
public record PatreonMember(string UserId, string? FullName, IReadOnlyList<string> TierTitles);
