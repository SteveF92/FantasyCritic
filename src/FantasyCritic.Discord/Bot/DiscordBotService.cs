using System.Reflection;
using Discord.WebSocket;
using Discord;
using Discord.Interactions;
using FantasyCritic.Discord.Commands;
using NodaTime;
using FantasyCritic.Discord.Interfaces;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using Microsoft.Extensions.Configuration;

namespace FantasyCritic.Discord.Bot;
public class DiscordBotService
{
    private readonly string _botToken;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _client;
    private readonly List<ICommand> _commandsList;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public DiscordBotService(
        IFantasyCriticRepo mySQLFantasyCriticRepo,
        IDiscordRepo mySQLDiscordRepo,
        IClock systemClock,
        GameSearchingService gameSearchingService,
        IDiscordParameterParser parameterParser,
        IDiscordFormatter discordFormatter,
        InteractionService interactionService,
        IServiceProvider serviceProvider,
        IConfigurationRoot configuration,
        DiscordSocketClient client)
    {
        _client = client;
        _configuration = configuration;
        _botToken = configuration["BotToken"];
        _discordFormatter = discordFormatter;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
        //_commandsList = new List<ICommand>
        //{
        //    new GetLeagueCommand(mySQLDiscordRepo, mySQLFantasyCriticRepo, systemClock, parameterParser, discordFormatter, baseAddress),
        //    new GetLeagueLinkCommand(mySQLDiscordRepo, systemClock, parameterParser, discordFormatter, baseAddress),
        //    new GetPublisherCommand(mySQLDiscordRepo, systemClock, parameterParser, discordFormatter, baseAddress),
        //    new GetGameCommand(mySQLDiscordRepo, systemClock, parameterParser, gameSearchingService, discordFormatter, baseAddress),
        //    new GetUpcomingGamesCommand(mySQLDiscordRepo, systemClock, discordFormatter, baseAddress),
        //    new GetRecentGamesCommand(mySQLDiscordRepo, systemClock, discordFormatter, baseAddress),
        //    new SetLeagueChannelCommand(mySQLDiscordRepo, systemClock, discordFormatter, mySQLFantasyCriticRepo),
        //    new RemoveLeagueCommand(mySQLDiscordRepo, systemClock, discordFormatter)
        //};
    }

    public async Task InitializeBotAsync()
    {
        //_client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        _client.Ready += Client_Ready;
        _interactionService.Log += Log;

        //_client.SlashCommandExecuted += SlashCommandHandler;
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
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
        //await _interactionService.RegisterCommandsGloballyAsync(true);
#if DEBUG
        await _interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(_configuration["DevDiscordServerId"]), true);
#else
            await _interactionService.RegisterCommandsGloballyAsync(true);
#endif
        //await _interactionService.AddCommandsGloballyAsync(true);

        //foreach (var command in _commandsList)
        //{
        //    var globalCommand = new SlashCommandBuilder()
        //        .WithName(command.Name)
        //        .WithDescription(command.Description);

        //    if (command.Options.Any())
        //    {
        //        foreach (var option in command.Options)
        //        {
        //            globalCommand.AddOption(option);
        //        }
        //    }

        //    try
        //    {
        //        await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        //    }
        //    catch (HttpException exception)
        //    {
        //        var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
        //        Serilog.Log.Error(json);
        //    }
        //}
    }
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        var commandToHandle = _commandsList.FirstOrDefault(c => c.Name == command.Data.Name);
        if (commandToHandle != null)
        {
            try
            {
                await commandToHandle.HandleCommand(command);
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, ex.Message);
                await command.RespondAsync(embed: _discordFormatter.BuildErrorEmbed(
                    "Error Executing Command",
                    "There was an error executing this command. Please try again.",
                    command.User));
            }
        }
    }
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
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
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}
