namespace FantasyCritic.Web.Models.Responses;

public class PublisherSingleSlotRequirementsViewModel
{
    public PublisherSingleSlotRequirementsViewModel()
    {

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

    public PublisherSingleSlotRequirementsViewModel(bool counterPick)
    {
        CounterPick = counterPick;
    }

    public IReadOnlyList<string> BannedTags { get; set; }
    public IReadOnlyList<string> RequiredTags { get; set; }
    public bool CounterPick { get; set; }

    public IReadOnlyList<LeagueTagStatus> GetLeagueTagStatus(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<LeagueTagStatus> domains = new List<LeagueTagStatus>();

        var bannedTagsWithoutRequiredTags = BannedTags.Except(RequiredTags);
        foreach (var bannedTag in bannedTagsWithoutRequiredTags)
        {
            bool hasTag = tagDictionary.TryGetValue(bannedTag, out var foundTag);
            if (!hasTag)
            {
                continue;
            }

            domains.Add(new LeagueTagStatus(foundTag, TagStatus.Banned));
        }

        foreach (var requiredTag in RequiredTags)
        {
            bool hasTag = tagDictionary.TryGetValue(requiredTag, out var foundTag);
            if (!hasTag)
            {
                continue;
            }

            domains.Add(new LeagueTagStatus(foundTag, TagStatus.Required));
        }

        return domains;
    }
}