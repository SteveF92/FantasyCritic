namespace FantasyCritic.Lib.GG;

public interface IGGService
{
    Task<GGGame?> GetGGGame(string ggToken);
}
