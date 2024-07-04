using FantasyCritic.Lib.Services;
using JetBrains.Annotations;
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

    [UsedImplicitly]
    public async Task Subscribe(string leagueID, int year)
    {
        try
        {
            Guid leagueGUID = Guid.Parse(leagueID);
            var draftIsActiveOrPaused = await _fantasyCriticService.DraftIsActiveOrPaused(leagueGUID, year);
            if (!draftIsActiveOrPaused)
            {
                return;
            }

            string groupName = $"{leagueGUID}|{year}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        catch (Exception e)
        {
            _logger.Error(e, "SignalR fail!");
            throw;
        }
    }
}
