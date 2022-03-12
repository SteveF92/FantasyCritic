namespace FantasyCritic.Lib.Domain;

public class TagOverride
{
    public TagOverride(MasterGame masterGame, IEnumerable<MasterGameTag> tags)
    {
        MasterGame = masterGame;
        Tags = tags.ToList();
    }

    public MasterGame MasterGame { get; }
    public IReadOnlyList<MasterGameTag> Tags { get; }
}