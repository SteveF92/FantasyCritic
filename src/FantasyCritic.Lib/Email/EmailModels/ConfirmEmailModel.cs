using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Email.EmailModels;

public class ConfirmEmailModel
{
    public ConfirmEmailModel(FantasyCriticUser user, string link)
    {
        DisplayName = user.UserName;
        Link = link;
    }

    public string DisplayName { get; }
    public string Link { get; }
}