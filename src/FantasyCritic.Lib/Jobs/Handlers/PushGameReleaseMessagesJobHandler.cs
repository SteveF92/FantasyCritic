using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class PushGameReleaseMessagesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.PushGameReleaseMessages;
    public static FantasyCriticJobPriority Priority => FantasyCriticJobPriority.StrictlyDependant;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.AtOnePastMidnightEastern;

    private readonly IMasterGameRepo _masterGameRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly IClock _clock;
    private readonly ILogger<PushGameReleaseMessagesJobHandler> _logger;

    public PushGameReleaseMessagesJobHandler(IMasterGameRepo masterGameRepo, DiscordPushService discordPushService,
        IClock clock, ILogger<PushGameReleaseMessagesJobHandler> logger)
    {
        _masterGameRepo = masterGameRepo;
        _discordPushService = discordPushService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var today = _clock.GetToday();
        var allMasterGames = await _masterGameRepo.GetMasterGameYears(today.Year);
        var masterGamesReleasingToday = allMasterGames.Where(x => x.MasterGame.ReleaseDate.HasValue && x.MasterGame.ReleaseDate.Value == today).ToList();
        if (!masterGamesReleasingToday.Any())
        {
            _logger.LogDebug("No games release on {Date}.", today.ToISOString());
            await context.UpdateDetailedStatus("No games released today.");
            return Result.Success();
        }

        cancellationToken.ThrowIfCancellationRequested();
        await _discordPushService.SendGameReleaseUpdates(masterGamesReleasingToday);

        var gameNames = masterGamesReleasingToday.Select(x => x.MasterGame.GameName).ToList();
        _logger.LogInformation("Pushed release messages for {GameCount} games releasing on {Date}: {GameNames}.", gameNames.Count, today.ToISOString(), gameNames);
        await context.UpdateDetailedStatus($"{gameNames.Count} games released today: {string.Join(", ", gameNames)}.");
        return Result.Success();
    }
}
