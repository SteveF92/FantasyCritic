using FantasyCritic.Lib.Identity;

namespace FantasyCritic.EmailTemplates.Models;

public class ChangeEmailModel
{
    public ChangeEmailModel(FantasyCriticUser user, string link)
    {
        DisplayName = user.UserName;
        Link = link;
    }

    public string DisplayName { get; }
    public string Link { get; }
}
