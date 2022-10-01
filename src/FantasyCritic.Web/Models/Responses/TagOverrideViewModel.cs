using FantasyCritic.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class TagOverrideViewModel
{
    public TagOverrideViewModel(TagOverride domain, LocalDate currentDate)
    {
        MasterGame = new MasterGameViewModel(domain.MasterGame, currentDate);
        Tags = domain.Tags.Select(x => x.Name).ToList();
    }

    public MasterGameViewModel MasterGame { get; }
    public List<string> Tags { get; }
}
