using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.SharedSerialization.API;

public class MinimalFantasyCriticUserViewModel
{
    public MinimalFantasyCriticUserViewModel()
    {

    }
    
    public MinimalFantasyCriticUserViewModel(VeryMinimalFantasyCriticUser user)
    {
        UserID = user.UserID;
        DisplayName = user.DisplayName;
    }

    public Guid UserID { get; init; }
    public string DisplayName { get; init; } = null!;

    public VeryMinimalFantasyCriticUser ToDomain()
    {
        return new VeryMinimalFantasyCriticUser(UserID, DisplayName);
    }
}
