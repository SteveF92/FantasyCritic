using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Domain;

public class MasterGameWithEligibilityFactors
{
    public MasterGameWithEligibilityFactors(MasterGame masterGame, LeagueOptions options, bool? overridenEligibility,
        IReadOnlyList<MasterGameTag> tagOverrides, LocalDate dateAcquired)
    {
        MasterGame = masterGame;
        Options = options;
        OverridenEligibility = overridenEligibility;
        TagOverrides = tagOverrides;
        DateAcquired = dateAcquired;
    }

    public MasterGame MasterGame { get; }
    public LeagueOptions Options { get; }
    public bool? OverridenEligibility { get; }
    public IReadOnlyList<MasterGameTag> TagOverrides { get; }
    public LocalDate DateAcquired { get; }

    public bool GameIsSpecificallyAllowed => OverridenEligibility.HasValue && OverridenEligibility.Value;
    public bool GameIsSpecificallyBanned => OverridenEligibility.HasValue && !OverridenEligibility.Value;

    public IReadOnlyList<ClaimError> CheckGameAgainstTags(IEnumerable<LeagueTagStatus> leagueTags, IEnumerable<LeagueTagStatus> slotTags)
    {
        var tagsToUse = TagOverrides.Any() ? TagOverrides : MasterGame.Tags;
        return LeagueTagExtensions.GameHasValidTags(leagueTags, slotTags, MasterGame, tagsToUse, DateAcquired);
    }

    public override string ToString() => MasterGame.GameName;
}
