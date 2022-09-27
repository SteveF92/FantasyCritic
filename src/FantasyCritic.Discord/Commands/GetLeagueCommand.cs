using Discord;
using Discord.WebSocket;

namespace FantasyCritic.Discord.Commands;
public class GetLeagueCommand : ICommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SlashCommandOptionBuilder[] Options { get; set; }

    public GetLeagueCommand()
    {
        Name = "league";
        Description = "Get league information.";
        Options = new SlashCommandOptionBuilder[]
        {
            //new()
            //{
            //    Name = "user",
            //    Description = "The users whose roles you want to be listed",
            //    Type = ApplicationCommandOptionType.User,
            //    IsRequired = true
            //}
        };
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {

        //var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

        //var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

        //var embedBuilder = new EmbedBuilder()
        //    .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
        //    .WithTitle("Roles")
        //    .WithDescription(roleList)
        //    .WithColor(Color.Green)
        //    .WithCurrentTimestamp();
        await command.RespondAsync("it worked", ephemeral: true);
    }
}
