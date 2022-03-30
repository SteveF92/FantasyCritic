using Newtonsoft.Json;

namespace FantasyCritic.Web.Models.RoundTrip;

public class SpecialGameSlotViewModel
{
    [JsonConstructor]
    public SpecialGameSlotViewModel(int specialSlotPosition, List<string> requiredTags)
    {
        SpecialSlotPosition = specialSlotPosition;
        RequiredTags = requiredTags;
    }

    public SpecialGameSlotViewModel(SpecialGameSlot domain)
    {
        SpecialSlotPosition = domain.SpecialSlotPosition;
        RequiredTags = domain.Tags.Select(x => x.Name).ToList();
    }

    public int SpecialSlotPosition { get; }
    public List<string> RequiredTags { get; }

    public SpecialGameSlot ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<MasterGameTag> tags = new List<MasterGameTag>();
        foreach (var tag in RequiredTags)
        {
            if (!tagDictionary.TryGetValue(tag, out var foundTag))
            {
                continue;
            }

            tags.Add(foundTag);
        }

        return new SpecialGameSlot(SpecialSlotPosition, tags);
    }
}
