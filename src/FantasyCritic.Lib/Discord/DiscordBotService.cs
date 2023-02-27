using Discord.WebSocket;
using Discord;
using Discord.Interactions;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Discord.EventHandlers;

namespace FantasyCritic.Lib.Discord;
public class DiscordBotService
{
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly FantasyCriticDiscordConfiguration _botConfiguration;
    private readonly SelectMenuExecutedHandler _selectMenuExecutedHandler;

    public DiscordBotService(InteractionService interactionService,
        IServiceProvider serviceProvider,
        FantasyCriticDiscordConfiguration botConfiguration,
        SelectMenuExecutedHandler selectMenuExecutedHandler,
        DiscordSocketClient client)
    {
        _client = client;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
        _botConfiguration = botConfiguration;
        _selectMenuExecutedHandler = selectMenuExecutedHandler;
    }

    public async Task InitializeBotAsync()
    {
        _client.Ready += Client_Ready;
        _interactionService.Log += Log;
        await _interactionService.AddModulesAsync(typeof(DiscordBotService).Assembly, _serviceProvider);
        _client.InteractionCreated += HandleInteraction;
        _client.SelectMenuExecuted += _selectMenuExecutedHandler.OnSelectMenuExecuted;

        await _client.LoginAsync(TokenType.Bot, _botConfiguration.BotToken);
        await _client.StartAsync();
    }

    private static Task Log(LogMessage msg)
    {
        Serilog.Log.Information(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task Client_Ready()
    {
        await _interactionService.RegisterCommandsGloballyAsync();
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        Serilog.Log.Error("Unmet Precondition {0}", result.Error.ToString());
                        break;
                    default:
                        break;
                }
            }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
