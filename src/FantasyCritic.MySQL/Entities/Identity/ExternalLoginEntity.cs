namespace FantasyCritic.MySQL.Entities.Identity;

public class ExternalLoginEntity
{
    public string LoginProvider { get; set; } = null!;
    public string ProviderKey { get; set; } = null!;
    public Guid UserID { get; set; }
    public string ProviderDisplayName { get; set; } = null!;
}
