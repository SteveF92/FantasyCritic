using Discord;
using Discord.WebSocket;

namespace FantasyCritic.Discord.Commands;
public interface ICommand
{
    string Name { get; set; }
    string Description { get; set; }
    SlashCommandOptionBuilder[] Options { get; set; }
    Task HandleCommand(SocketSlashCommand command);
}
