using FantasyCritic.Lib.Patreon;

namespace FantasyCritic.MySQL.Entities;
public class PatreonTokensEntity
{
    public PatreonTokensEntity()
    {
        
    }

    public PatreonTokensEntity(PatreonTokens domain)
    {
        AccessToken = domain.AccessToken;
        RefreshToken = domain.RefreshToken;
    }

    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;

    public PatreonTokens ToDomain()
    {
        return new PatreonTokens(AccessToken, RefreshToken);
    }
}
