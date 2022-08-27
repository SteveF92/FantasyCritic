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
        var botToken = "TOKEN";

        _client = new DiscordSocketClient();
        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
