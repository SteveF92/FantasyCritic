namespace FantasyCritic.Web.Models.RoundTrip;

public class LeagueTagOptionsViewModel
{
    public LeagueTagOptionsViewModel(List<string> banned, List<string> required)
    {
        Banned = banned;
        Required = required;
    }

    public List<string> Banned { get; }
    public List<string> Required { get; }

    public IReadOnlyList<LeagueTagStatus> ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<LeagueTagStatus> leagueTags = new List<LeagueTagStatus>();
        foreach (var bannedTag in Banned)
        {
            bool hasTag = tagDictionary.TryGetValue(bannedTag, out var foundTag);
            if (!hasTag)
            {
                continue;
            }

            leagueTags.Add(new LeagueTagStatus(foundTag, TagStatus.Banned));
        }
        foreach (var requiredTag in Required)
        {
            bool hasTag = tagDictionary.TryGetValue(requiredTag, out var foundTag);
            if (!hasTag)
            {
                continue;
            }

            leagueTags.Add(new LeagueTagStatus(foundTag, TagStatus.Required));
        }

        return leagueTags;
    }
}
