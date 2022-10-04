using Discord;
using Discord.WebSocket;

namespace FantasyCritic.Discord.Commands;
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    SlashCommandOptionBuilder[] Options { get; }
    Task HandleCommand(SocketSlashCommand command);
}
