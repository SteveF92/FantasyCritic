using FantasyCritic.Lib.Identity;

namespace FantasyCritic.SharedSerialization.API;

public class MinimalFantasyCriticUserViewModel
{
    public MinimalFantasyCriticUserViewModel()
    {

    }
    
    public MinimalFantasyCriticUserViewModel(MinimalFantasyCriticUser user)
    {
        UserID = user.UserID;
        DisplayName = user.DisplayName;
        EmailAddress = user.EmailAddress;
    }

    public Guid UserID { get; init; }
    public string DisplayName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;

    public MinimalFantasyCriticUser ToDomain()
    {
        return new MinimalFantasyCriticUser(UserID, DisplayName, EmailAddress);
    }
}
