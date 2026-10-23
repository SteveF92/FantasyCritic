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
        SpecialSlotPosition = domain.GetOverallSlotNumber(leagueOptions);
        RequiredTags = domain.RequiredTags.Select(x => x.Name).ToList();
    }

    public int SpecialSlotPosition { get; }
    public List<string> RequiredTags { get; }
}
