namespace FantasyCritic.Worker;

public class Scheduler : BackgroundService
{
    private readonly ILogger<Scheduler> _logger;

    public Scheduler(ILogger<Scheduler> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
