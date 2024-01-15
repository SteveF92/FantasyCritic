using Discord.Interactions;
using Discord.WebSocket;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.EventHandlers;
public class NotableMissesSelectMenuHandler : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;

    public NotableMissesSelectMenuHandler(IDiscordRepo discordRepo, InterLeagueService interLeagueService)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
    }

    [ComponentInteraction("notable-misses")]
    public async Task NotableMissesSelectMenu(string[] selectedOptions)
    {
        await DeferAsync();

        if (selectedOptions.Any())
        {
            var valueToSet = selectedOptions.First().ToLower() == "yes";
            if (Context.Channel is not SocketGuildChannel guildChannel)
            {
                await FollowupAsync("There was an error saving your setting. Please try again or contact support.");
                return;
            }

            var supportedYears = await _interLeagueService.GetSupportedYears();
            var leagueGameNewsChannel = await _discordRepo.GetLeagueChannel(guildChannel.Guild.Id, guildChannel.Id, supportedYears);
            if (leagueGameNewsChannel == null)
            {
                return;
            }

            await _discordRepo.SetLeagueGameNewsSetting(leagueGameNewsChannel.LeagueYear.League.LeagueID,
                leagueGameNewsChannel.GuildID,
                leagueGameNewsChannel.ChannelID,
                leagueGameNewsChannel.SendLeagueMasterGameUpdates,
                valueToSet);
            await DeleteOriginalResponseAsync();
            await FollowupAsync($"Notable Misses has been set to **{(valueToSet ? "ON" : "OFF")}**");
        }
    }
}
