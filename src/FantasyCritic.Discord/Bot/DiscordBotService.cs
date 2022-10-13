using System.Reflection;
using Discord.WebSocket;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;

namespace FantasyCritic.Discord.Bot;
public class DiscordBotService
{
    private readonly string _botToken;
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public DiscordBotService(InteractionService interactionService,
        IServiceProvider serviceProvider,
        IConfigurationRoot configuration,
        DiscordSocketClient client)
    {
        _client = client;
        _configuration = configuration;
        _botToken = configuration["BotToken"];
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeBotAsync()
    {
        _client.Ready += Client_Ready;
        _interactionService.Log += Log;
        await _interactionService.AddModulesAsync(typeof(DiscordBotService).Assembly, _serviceProvider);
        _client.InteractionCreated += HandleInteraction;

        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();
    }

    private static Task Log(LogMessage msg)
    {
        Serilog.Log.Information(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task Client_Ready()
    {
#if DEBUG
        await _interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(_configuration["DevDiscordServerId"]), true);
#else
            await _interactionService.RegisterCommandsGloballyAsync(true);
#endif
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        Serilog.Log.Error("Unmet Precondition {0}", result.Error.ToString());
                        break;
                    default:
                        break;
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
