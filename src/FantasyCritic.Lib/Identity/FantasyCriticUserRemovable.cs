namespace FantasyCritic.Lib.Identity;

public class FantasyCriticUserRemovable
{
    public FantasyCriticUserRemovable(FantasyCriticUser user, bool removable)
    {
        User = user;
        Removable = removable;
    }

    public FantasyCriticUser User { get; }
    public bool Removable { get; }
}
