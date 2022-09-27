using System.Reflection;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using FantasyCritic.Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FantasyCritic.DiscordBot;

using System;

public class Program
{
    private DiscordSocketClient _client = null!;
    private readonly List<ICommand> _commandsList = new()
    {
        new GetLeagueCommand()
    };

    public static Task Main() => new Program().MainAsync();
    public async Task MainAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        var botToken = configuration["BotToken"];

        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
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
                Console.WriteLine(json);
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
