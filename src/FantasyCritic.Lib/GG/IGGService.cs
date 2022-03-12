namespace FantasyCritic.Lib.GG
{
    public interface IGGService
    {
        Task<Maybe<GGGame>> GetGGGame(string ggToken);
    }
}
