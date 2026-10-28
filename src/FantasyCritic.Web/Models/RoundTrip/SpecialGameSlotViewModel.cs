using System.Text.Json.Serialization;

namespace FantasyCritic.Web.Models.RoundTrip;

public class SpecialGameSlotViewModel
{
    [JsonConstructor]
    public SpecialGameSlotViewModel(int specialSlotPosition, List<string> requiredTags)
    {
        SpecialSlotPosition = specialSlotPosition;
        RequiredTags = requiredTags;
    }

    public SpecialGameSlotViewModel(LeagueSlot domain, LeagueOptions leagueOptions)
    {
        SpecialSlotPosition = domain.GetSpecialSlotPosition(leagueOptions) ?? throw new ArgumentException($"Slot {domain} is not a special slot.", nameof(domain));
        RequiredTags = domain.RequiredTags.Select(x => x.Name).ToList();
    }

    public int SpecialSlotPosition { get; }
    public List<string> RequiredTags { get; }

    public IReadOnlyList<MasterGameTag> GetRequiredTags(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<MasterGameTag> tags = [];
        foreach (var tag in RequiredTags)
        {
            var foundTag = tagDictionary.GetValueOrDefault(tag);
            if (foundTag is null)
            {
                continue;
            }

            tags.Add(foundTag);
        }

        return tags;
    }
}
