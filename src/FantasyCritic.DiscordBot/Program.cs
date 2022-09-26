using Discord.Net;
using Newtonsoft.Json;

namespace FantasyCritic.DiscordBot;

using Discord;
using Discord.WebSocket;
using System;

public class Program
{
    private DiscordSocketClient _client = null!;

    public static Task Main() => new Program().MainAsync();
    public async Task MainAsync()
    {
        var botToken = "";

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
        var globalCommand = new SlashCommandBuilder()
            .WithName("list-roles")
            .WithDescription("Lists all roles of a user")
            .AddOption("user", ApplicationCommandOptionType.User, "The users whose roles you want to be listed", isRequired:true);

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

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "list-roles":
                await HandleListRoleCommand(command);
                break;
        }
    }

    private async Task HandleListRoleCommand(SocketSlashCommand command)
    {
        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

        var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Roles")
            .WithDescription(roleList)
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
    }
}
