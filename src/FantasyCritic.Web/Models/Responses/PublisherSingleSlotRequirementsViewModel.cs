namespace FantasyCritic.Web.Models.Responses;

public class PublisherSingleSlotRequirementsViewModel
{
    public PublisherSingleSlotRequirementsViewModel()
    {
        BannedTags = new List<string>();
        RequiredTags = new List<string>();
    }

    public PublisherSingleSlotRequirementsViewModel(IEnumerable<LeagueTagStatus> tags)
    {
        BannedTags = tags.Where(x => x.Status.Equals(TagStatus.Banned)).Select(x => x.Tag.Name).ToList();
        RequiredTags = tags.Where(x => x.Status.Equals(TagStatus.Required)).Select(x => x.Tag.Name).ToList();
    }

    public PublisherSingleSlotRequirementsViewModel(IEnumerable<string> bannedTags)
    {
        BannedTags = bannedTags.ToList();
        RequiredTags = new List<string>();
    }

    public IReadOnlyList<string> BannedTags { get; init; }
    public IReadOnlyList<string> RequiredTags { get; init; }
    public bool CounterPick { get; init; }

    public IReadOnlyList<LeagueTagStatus> GetLeagueTagStatus(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<LeagueTagStatus> domains = new List<LeagueTagStatus>();

        var bannedTagsWithoutRequiredTags = BannedTags.Except(RequiredTags);
        foreach (var bannedTag in bannedTagsWithoutRequiredTags)
        {
            var foundTag = tagDictionary.GetValueOrDefault(bannedTag);
            if (foundTag is null)
            {
                continue;
            }

            domains.Add(new LeagueTagStatus(foundTag, TagStatus.Banned));
        }

        foreach (var requiredTag in RequiredTags)
        {
            var foundTag = tagDictionary.GetValueOrDefault(requiredTag);
            if (foundTag is null)
            {
                continue;
            }

            domains.Add(new LeagueTagStatus(foundTag, TagStatus.Required));
        }

        return domains;
    }
}
