namespace FantasyCritic.Lib.OpenCritic
{
    public interface IOpenCriticService
    {
        Task<Maybe<OpenCriticGame>> GetOpenCriticGame(int openCriticGameID);
    }
}
