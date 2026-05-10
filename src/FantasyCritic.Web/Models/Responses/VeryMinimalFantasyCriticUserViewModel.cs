using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class VeryMinimalFantasyCriticUserViewModel
{
    public VeryMinimalFantasyCriticUserViewModel(IVeryMinimalFantasyCriticUser domain)
    {
        UserID = domain.UserID;
        DisplayName = domain.DisplayName;
    }

    public Guid UserID { get; }
    public string DisplayName { get; }
}
