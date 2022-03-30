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
            if (!tagDictionary.TryGetValue(bannedTag, out var foundTag))
            {
                continue;
            }

            leagueTags.Add(new LeagueTagStatus(foundTag, TagStatus.Banned));
        }
        foreach (var requiredTag in Required)
        {
            if (!tagDictionary.TryGetValue(requiredTag, out var foundTag))
            {
                continue;
            }

            leagueTags.Add(new LeagueTagStatus(foundTag, TagStatus.Required));
        }

        return leagueTags;
    }
}
