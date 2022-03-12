namespace FantasyCritic.Web.Models.RoundTrip;

public class SpecialGameSlotViewModel
{
    public SpecialGameSlotViewModel()
    {

    }

    public SpecialGameSlotViewModel(SpecialGameSlot domain)
    {
        SpecialSlotPosition = domain.SpecialSlotPosition;
        RequiredTags = domain.Tags.Select(x => x.Name).ToList();
    }

    public int SpecialSlotPosition { get; set; }
    public List<string> RequiredTags { get; set; }

    public SpecialGameSlot ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<MasterGameTag> tags = new List<MasterGameTag>();
        foreach (var tag in RequiredTags)
        {
            bool hasTag = tagDictionary.TryGetValue(tag, out var foundTag);
            if (!hasTag)
            {
                continue;
            }

            tags.Add(foundTag);
        }

        return new SpecialGameSlot(SpecialSlotPosition, tags);
    }
}