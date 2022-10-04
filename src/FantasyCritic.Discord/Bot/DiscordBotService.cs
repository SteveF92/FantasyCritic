using Discord.WebSocket;
using Discord;
using FantasyCritic.Discord.Commands;
using NodaTime;
using Discord.Net;
using Newtonsoft.Json;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Discord.Bot;
public class DiscordBotService
{
    private readonly string _botToken;
    private readonly IFantasyCriticRepo _mySQLFantasyCriticRepo;
    private readonly IDiscordRepo _mySQLDiscordRepo;
    private readonly SystemClock _systemClock;
    private readonly IParameterParser _parameterParser;
    private readonly string _baseUrl;
    private DiscordSocketClient _client = null!;
    private List<ICommand> _commandsList = new();

    public DiscordBotService(string botToken,
        IFantasyCriticRepo mySQLFantasyCriticRepo,
        IDiscordRepo mySQLDiscordRepo,
        SystemClock systemClock,
        IParameterParser parameterParser,
        string baseUrl)
    {
        _botToken = botToken;
        _mySQLFantasyCriticRepo = mySQLFantasyCriticRepo;
        _mySQLDiscordRepo = mySQLDiscordRepo;
        _systemClock = systemClock;
        _parameterParser = parameterParser;
        _baseUrl = baseUrl;
    }

    public async Task InitializeBot()
    {
        _commandsList = new List<ICommand>
        {
            new GetLeagueCommand(_mySQLDiscordRepo, _mySQLFantasyCriticRepo, _systemClock, _parameterParser, _baseUrl),
            new GetLeagueLinkCommand(_mySQLDiscordRepo, _systemClock, _parameterParser, _baseUrl),
            new GetPublisherCommand(_mySQLDiscordRepo, _systemClock, _parameterParser, _baseUrl)
        };

        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

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
        foreach (var command in _commandsList)
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName(command.Name)
                .WithDescription(command.Description);

            if (command.Options.Any())
            {
                foreach (var option in command.Options)
                {
                    globalCommand.AddOption(option);
                }
            }

            try
            {
                await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Serilog.Log.Error(json);
            }
        }
    }
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        var commandToHandle = _commandsList.FirstOrDefault(c => c.Name == command.Data.Name);
        if (commandToHandle != null)
        {
            await commandToHandle.HandleCommand(command);
        }
    }
}
