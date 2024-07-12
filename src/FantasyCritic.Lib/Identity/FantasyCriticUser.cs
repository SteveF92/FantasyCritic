using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Identity;

public class FantasyCriticUser : IdentityUser<Guid>, IMinimalFantasyCriticUser
{
    public FantasyCriticUser()
    {
        GeneralUserSettings = GeneralUserSettings.Default;
    }

    public FantasyCriticUser(Guid userID, string displayName, string? patreonDonorNameOverride, int displayNumber, string emailAddress, string normalizedEmailAddress,
        bool emailConfirmed, string? securityStamp, string? passwordHash, bool twoFactorEnabled, string? authenticatorKey, Instant lastChangedCredentials,
        GeneralUserSettings generalUserSettings, bool isDeleted)
    {
        Id = userID;
        base.UserName = displayName;
        PatreonDonorNameOverride = patreonDonorNameOverride;
        DisplayNumber = displayNumber;
        base.Email = emailAddress;
        base.NormalizedEmail = normalizedEmailAddress;
        EmailConfirmed = emailConfirmed;
        SecurityStamp = securityStamp;
        PasswordHash = passwordHash;
        TwoFactorEnabled = twoFactorEnabled;
        AuthenticatorKey = authenticatorKey;
        LastChangedCredentials = lastChangedCredentials;
        GeneralUserSettings = generalUserSettings;
        IsDeleted = isDeleted;
    }

    public new string UserName
    {
        get => base.UserName!;
        set => base.UserName = value;
    }

    public new string Email
    {
        get => base.Email!;
        set => base.Email = value;
    }

    public new string NormalizedEmail
    {
        get => base.NormalizedEmail!;
        set => base.NormalizedEmail = value;
    }

    public string? PatreonDonorNameOverride { get; }
    public int DisplayNumber { get; set; }
    public Instant LastChangedCredentials { get; set; }
    public GeneralUserSettings GeneralUserSettings { get; }
    public bool IsDeleted { get; set; }
    public string? AuthenticatorKey { get; set; }

    public void UpdateLastUsedCredentials(Instant currentInstant)
    {
        LastChangedCredentials = currentInstant;
    }

    public FantasyCriticUser UpdateGeneralSettings(GeneralUserSettings generalSettings)
    {
        return new FantasyCriticUser(Id, UserName, PatreonDonorNameOverride, DisplayNumber, Email, NormalizedEmail, EmailConfirmed,
            SecurityStamp, PasswordHash, TwoFactorEnabled, AuthenticatorKey, LastChangedCredentials, generalSettings, IsDeleted);
    }

    public MinimalFantasyCriticUser ToMinimal() => new MinimalFantasyCriticUser(Id, UserName, Email);
    public VeryMinimalFantasyCriticUser ToVeryMinimal() => new VeryMinimalFantasyCriticUser(Id, UserName);

    public virtual bool Equals(IVeryMinimalFantasyCriticUser? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return UserID.Equals(other.UserID);
    }

    public override bool Equals(object? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (other is not IVeryMinimalFantasyCriticUser otherUser)
        {
            return false;
        }

        return UserID.Equals(otherUser.UserID);
    }


    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static FantasyCriticUser GetFakeUser()
    {
        return new FantasyCriticUser(Guid.Empty, "<Non-Existent User>", null, 0, "", "", false, "", "", false, null,
            Instant.MinValue, GeneralUserSettings.Default, false);
    }

    public Guid UserID => Id;
    public string DisplayName => UserName;
    public string EmailAddress => Email;
}
