using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace FantasyCritic.Web.Hubs;

public class UpdateHub : Hub
{
    private readonly FantasyCriticService _fantasyCriticService;
    private static readonly ILogger _logger = Log.ForContext<UpdateHub>();

    public UpdateHub(FantasyCriticService fantasyCriticService)
    {
        _fantasyCriticService = fantasyCriticService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (ex is not null)
        {
            _logger.Error(ex, $"SignalR disconnected with error: {Context.ConnectionId}");
        }

        await base.OnDisconnectedAsync(ex);
    }

    public async Task Subscribe(string leagueID, int year)
    {
        try
        {
            Guid leagueGUID = Guid.Parse(leagueID);
            var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueGUID, year);
            if (leagueYear is null)
            {
                return;
            }

            if (!leagueYear.PlayStatus.DraftIsActiveOrPaused)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, leagueYear.GetGroupName);
        }
        catch (Exception e)
        {
            _logger.Error(e, "SignalR fail!");
            throw;
        }
    }
}
