namespace FantasyCritic.Lib.OpenCritic;

public interface IOpenCriticService
{
    Task<OpenCriticGame?> GetOpenCriticGame(MasterGame masterGame, CancellationToken cancellationToken);
    Task<OpenCriticGame?> GetOpenCriticGame(MasterSubGame masterGame, CancellationToken cancellationToken);
}
