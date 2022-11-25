using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Email.EmailModels;

public class PasswordResetModel
{
    public PasswordResetModel(FantasyCriticUser user, string link)
    {
        DisplayName = user.GetUserName();
        Link = link;
    }

    public string DisplayName { get; }
    public string Link { get; }
}
