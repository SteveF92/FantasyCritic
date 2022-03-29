namespace FantasyCritic.Lib.OpenCritic;

public interface IOpenCriticService
{
    Task<OpenCriticGame?> GetOpenCriticGame(int openCriticGameID);
}
