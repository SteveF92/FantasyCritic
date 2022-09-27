using FantasyCritic.Lib.Patreon;

namespace FantasyCritic.Lib.Interfaces;
public interface IPatreonTokensRepo
{
    Task SaveTokens(PatreonTokens keys);
    Task<PatreonTokens> GetMostRecentTokens();
}
