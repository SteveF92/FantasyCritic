using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace FantasyCritic.Web.Hubs;

public class UpdateHub : Hub
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly FantasyCriticService _fantasyCriticService;

    public UpdateHub(FantasyCriticService fantasyCriticService)
    {
        _fantasyCriticService = fantasyCriticService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception ex)
    {
        if (ex is not null)
        {
            _logger.Error($"SignalR disconnected with error: {Context.ConnectionId}");
            _logger.Error(ex);
        }

        await base.OnDisconnectedAsync(ex);
    }

    public async Task Subscribe(string leagueID, string year)
    {
        try
        {
            Guid leagueGUID = Guid.Parse(leagueID);
            int yearInt = int.Parse(year);
            var leagueYear = await _fantasyCriticService.GetLeagueYear(leagueGUID, yearInt);
            if (leagueYear.HasNoValue)
            {
                return;
            }

            if (!leagueYear.Value.PlayStatus.DraftIsActive)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, leagueYear.Value.GetGroupName);
        }
        catch (Exception e)
        {
            _logger.Error("SignalR fail!");
            _logger.Error(e);
            throw;
        }

    }
}