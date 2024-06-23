using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.SharedSerialization.Database;

public class FantasyCriticUserEntity
{
    public FantasyCriticUserEntity()
    {

    }

    public FantasyCriticUserEntity(FantasyCriticUser user)
    {
        UserID = user.Id;
        DisplayName = user.UserName;
        PatreonDonorNameOverride = user.PatreonDonorNameOverride;
        DisplayNumber = user.DisplayNumber;
        EmailAddress = user.Email;
        NormalizedEmailAddress = user.NormalizedEmail;
        EmailConfirmed = user.EmailConfirmed;
        SecurityStamp = user.SecurityStamp;
        PasswordHash = user.PasswordHash;
        TwoFactorEnabled = user.TwoFactorEnabled;
        AuthenticatorKey = user.AuthenticatorKey;
        LastChangedCredentials = user.LastChangedCredentials;
        ShowDecimalPoints = user.GeneralUserSettings.ShowDecimalPoints;
        IsDeleted = user.IsDeleted;
    }

    public Guid UserID { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? PatreonDonorNameOverride { get; set; }
    public int DisplayNumber { get; set; }
    public string EmailAddress { get; set; } = null!;
    public string NormalizedEmailAddress { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string? SecurityStamp { get; set; }
    public string? PasswordHash { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? AuthenticatorKey { get; set; }
    public Instant LastChangedCredentials { get; set; }
    public bool ShowDecimalPoints { get; set; }
    public bool IsDeleted { get; set; }

    public FantasyCriticUser ToDomain()
    {
        var generalSettings = new GeneralUserSettings(ShowDecimalPoints);
        FantasyCriticUser domain = new FantasyCriticUser(UserID, DisplayName, PatreonDonorNameOverride, DisplayNumber, EmailAddress, NormalizedEmailAddress, EmailConfirmed,
            SecurityStamp, PasswordHash, TwoFactorEnabled, AuthenticatorKey, LastChangedCredentials, generalSettings, IsDeleted);
        return domain;
    }
}
