using FantasyCritic.Lib.Identity;

namespace FantasyCritic.EmailTemplates.Models;

public class PasswordResetModel
{
    public PasswordResetModel(FantasyCriticUser user, string link)
    {
        DisplayName = user.UserName;
        Link = link;
    }

    public string DisplayName { get; }
    public string Link { get; }
}
