using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.SharedSerialization.API;

public class FantasyCriticUserViewModel
{
    public FantasyCriticUserViewModel()
    {

    }
    
    public FantasyCriticUserViewModel(FantasyCriticUser user, IEnumerable<string> roles)
    {
        UserID = user.Id;
        DisplayName = user.UserName;
        DisplayNumber = user.DisplayNumber;
        EmailAddress = user.Email;
        Roles = roles;
        EmailConfirmed = user.EmailConfirmed;
        ShowDecimalPoints = user.GeneralUserSettings.ShowDecimalPoints;
    }

    public FantasyCriticUserViewModel(FantasyCriticUser user)
        : this(user, new List<string>())
    {

    }

    public Guid UserID { get; init; }
    public string DisplayName { get; init; } = null!;
    public int DisplayNumber { get; init; }
    public string EmailAddress { get; init; } = null!;
    public IEnumerable<string> Roles { get; init; } = null!;
    public bool EmailConfirmed { get; init; }
    public bool ShowDecimalPoints { get; init; }

    public FantasyCriticUser ToDomain()
    {
        var generalSettings = new GeneralUserSettings(ShowDecimalPoints);
        return new FantasyCriticUser(UserID, DisplayName, null, DisplayNumber, EmailAddress, EmailAddress, EmailConfirmed, "", "", false, null, Instant.MinValue, generalSettings, false);
    }
}
