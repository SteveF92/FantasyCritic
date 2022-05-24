using FantasyCritic.Lib.Services;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace FantasyCritic.Web.Hubs;

public class UpdateHub : Hub
{
    private readonly FantasyCriticService _fantasyCriticService;

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
            Log.Error(ex, $"SignalR disconnected with error: {Context.ConnectionId}");
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
            if (leagueYear is null)
            {
                return;
            }

            if (!leagueYear.PlayStatus.DraftIsActive)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, leagueYear.GetGroupName);
        }
        catch (Exception e)
        {
            Log.Error(e, "SignalR fail!");
            throw;
        }
    }
}
