namespace FantasyCritic.Lib.Domain.LeagueActions;


public record LeagueOptionsDifferences(IReadOnlyList<string> Differences)
{
    public bool HasChanges => Differences.Any();

    public override string ToString()
    {
        if (!HasChanges)
        {
            return string.Empty;
        }

        if (Differences.Count == 1)
        {
            return Differences.Single();
        }

        return string.Join("\n", Differences.Select(x => $"• {x}"));
    }

    public LeagueOptionsDifferences Combine(LeagueOptionsDifferences other)
    {
        return new LeagueOptionsDifferences(Differences.Concat(other.Differences).ToList());
    }
}
