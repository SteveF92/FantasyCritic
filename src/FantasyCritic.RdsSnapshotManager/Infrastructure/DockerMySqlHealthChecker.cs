using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace FantasyCritic.RdsSnapshotManager.Infrastructure;

public sealed class DockerMySqlHealthChecker
{
    public async Task<Result> EnsureHealthy(string containerName, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"inspect -f \"{{{{.State.Health.Status}}}}\" {containerName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start docker inspect.");

        var status = (await process.StandardOutput.ReadToEndAsync(cancellationToken)).Trim();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            return Result.Failure($"docker inspect failed for container {containerName}.");
        }

        return status == "healthy"
            ? Result.Success()
            : Result.Failure($"Container {containerName} is not healthy (status: {status}).");
    }
}
