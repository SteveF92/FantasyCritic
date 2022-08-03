using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Services;
using Serilog;

namespace FantasyCritic.Lib.Discord;
public class DiscordRequestService : BaseDiscordService
{
    private readonly InterLeagueService _interLeagueService;
    private static readonly ILogger _logger = Log.ForContext<DiscordRequestService>();

    public DiscordRequestService(DiscordConfiguration configuration, InterLeagueService interLeagueService)
        : base(configuration)
    {
        _interLeagueService = interLeagueService;
    }

    public async Task HandleRequests()
    {
        _logger.Information("Handling requests");
        bool isMasterGameRequest = true;
        if (isMasterGameRequest)
        {
            await RespondToMasterGameRequest();
        }
    }

    private async Task RespondToMasterGameRequest()
    {
        var masterGame = await _interLeagueService.GetMasterGame(Guid.Parse("b029d015-a150-4fc2-afea-fca18d184a75"));
        if (masterGame is null)
        {
            await SendMessage("That game does not exist.");
            return;
        }

        await SendMessage($"That game is: {masterGame.GameName}");
    }
}
