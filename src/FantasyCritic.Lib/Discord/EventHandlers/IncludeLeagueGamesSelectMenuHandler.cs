using Discord.Interactions;
using Discord.WebSocket;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using JetBrains.Annotations;

namespace FantasyCritic.Lib.Discord.EventHandlers;
public class IncludeLeagueGamesSelectMenuHandler : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;

    public IncludeLeagueGamesSelectMenuHandler(IDiscordRepo discordRepo, InterLeagueService interLeagueService)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
    }

    [UsedImplicitly]
    [ComponentInteraction("include-league-games")]
    public async Task IncludeLeagueGamesSelectMenu(string[] selectedOptions)
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
                valueToSet,
                leagueGameNewsChannel.SendNotableMisses);
            await DeleteOriginalResponseAsync();
            await FollowupAsync(
                $"League Game Updates has been set to **{(valueToSet ? "ON" : "OFF")}**");
        }
    }
}
