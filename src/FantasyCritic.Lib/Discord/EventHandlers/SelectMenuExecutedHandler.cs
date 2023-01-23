using Discord.WebSocket;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.EventHandlers;
public class SelectMenuExecutedHandler
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;

    public SelectMenuExecutedHandler(IDiscordRepo discordRepo, InterLeagueService interLeagueService)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
    }

    public async Task OnSelectMenuExecuted(SocketMessageComponent messageComponent)
    {
        await messageComponent.DeferAsync();

        if (messageComponent.Data.Values.Any())
        {
            var valueToSet = messageComponent.Data.Values.First().ToLower() == "yes";
            if (messageComponent.Channel is not SocketGuildChannel guildChannel)
            {
                await messageComponent.FollowupAsync("There was an error saving your setting. Please try again or contact support.");
                return;
            }

            var supportedYears = await _interLeagueService.GetSupportedYears();
            var leagueGameNewsChannel = await _discordRepo.GetLeagueChannel(guildChannel.Guild.Id, guildChannel.Id, supportedYears);
            if (leagueGameNewsChannel == null)
            {
                return;
            }

            switch (messageComponent.Data.CustomId)
            {
                case "include-league-games":
                    await _discordRepo.SetLeagueGameNewsSetting(leagueGameNewsChannel.LeagueYear.League.LeagueID,
                        leagueGameNewsChannel.GuildID,
                        leagueGameNewsChannel.ChannelID,
                        valueToSet,
                        leagueGameNewsChannel.SendNotableMisses);
                    await messageComponent.FollowupAsync(
                        $"League Game Updates has been set to **{(valueToSet ? "ON" : "OFF")}**");
                    await messageComponent.DeleteOriginalResponseAsync();
                    break;
                case "notable-misses":
                    await _discordRepo.SetLeagueGameNewsSetting(leagueGameNewsChannel.LeagueYear.League.LeagueID,
                        leagueGameNewsChannel.GuildID,
                        leagueGameNewsChannel.ChannelID,
                        leagueGameNewsChannel.SendLeagueMasterGameUpdates,
                        valueToSet);
                    await messageComponent.DeleteOriginalResponseAsync();
                    await messageComponent.FollowupAsync(
                        $"Notable Misses has been set to **{(valueToSet ? "ON" : "OFF")}**");
                    break;
            }
        }
    }
}
