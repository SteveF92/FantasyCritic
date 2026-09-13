using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Jobs;
using NodaTime;

namespace FantasyCritic.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cancellationLoop = Task.Run(async () => await JobRunnerLoop(stoppingToken), CancellationToken.None);
        var executionLoop = Task.Run(async () => await JobCancellationLoop(stoppingToken), CancellationToken.None);

        await Task.WhenAll(cancellationLoop, executionLoop);
    }

    private async Task JobRunnerLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
           
        }
    }

    private async Task JobCancellationLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
       
        }
    }
}
