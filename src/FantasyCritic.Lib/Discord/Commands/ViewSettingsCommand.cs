using Discord;
using Discord.Interactions;
using DiscordDotNetUtilities.Interfaces;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Discord.UrlBuilders;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Discord.Commands;
public class ViewSettingsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IDiscordRepo _discordRepo;
    private readonly InterLeagueService _interLeagueService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string _baseAddress;

    public ViewSettingsCommand(IDiscordRepo discordRepo,
        InterLeagueService interLeagueService,
        IDiscordFormatter discordFormatter,
        FantasyCriticSettings fantasyCriticSettings)
    {
        _discordRepo = discordRepo;
        _interLeagueService = interLeagueService;
        _discordFormatter = discordFormatter;
        _baseAddress = fantasyCriticSettings.BaseAddress;
    }

    [SlashCommand("view-settings", "View current bot settings.")]
    public async Task ViewSettings()
    {
        await DeferAsync();

        var supportedYears = await _interLeagueService.GetSupportedYears();
        var leagueChannel = await _discordRepo.GetLeagueChannel(Context.Guild.Id, Context.Channel.Id, supportedYears);
        var gameNewsChannel = await _discordRepo.GetGameNewsChannel(Context.Guild.Id, Context.Channel.Id);

        var embedFieldBuilders = new List<EmbedFieldBuilder>();

        var leagueDisplay = leagueChannel?.LeagueYear != null
            ? new LeagueUrlBuilder(_baseAddress, leagueChannel.LeagueYear.League.LeagueID,
                leagueChannel.LeagueYear.Year).BuildUrl(leagueChannel.LeagueYear.League.LeagueName)
            : "No league has been set. Use `/set-league` to configure a league.";

        var settingsMessage = $"League: **{leagueDisplay}**\n";

        embedFieldBuilders.Add(new EmbedFieldBuilder
        {
            Name = "League",
            Value = leagueDisplay,
            IsInline = false
        });

        if (leagueChannel?.LeagueYear != null)
        {
            embedFieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "Game News",
                Value = GetGameNewsSettingDescription(leagueChannel.SendLeagueMasterGameUpdates, gameNewsChannel?.GameNewsSetting),
                IsInline = false
            });

            var publicBiddingRoleDisplay =
                GetPublicBiddingRoleDisplayText(leagueChannel.LeagueYear.Options.PickupSystem,
                    leagueChannel.BidAlertRoleID);
            embedFieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "Public Bid Role",
                Value = publicBiddingRoleDisplay,
                IsInline = false
            });
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed(
            "Fantasy Critic Bot Settings",
            settingsMessage,
            Context.User,
            embedFieldBuilders));
    }

    private static string GetGameNewsSettingDescription(bool sendLeagueMasterGameUpdates, GameNewsSetting? gameNewsSetting)
    {
        List<string> parts = new List<string>();
        if (sendLeagueMasterGameUpdates)
        {
            parts.Add("League Master Game Updates");
        }
        else
        {
            parts.Add("No League Master Game Updates");
        }

        if (gameNewsSetting is null)
        {
            parts.Add("No Non-League Master Game Updates");
        }
        else if (gameNewsSetting.Equals(GameNewsSetting.All))
        {
            parts.Add("All Master Game Updates");
        }
        else if (gameNewsSetting.Equals(GameNewsSetting.MightReleaseInYear))
        {
            parts.Add("Any 'Might Release' Master Game Updates");
        }
        else if (gameNewsSetting.Equals(GameNewsSetting.WillReleaseInYear))
        {
            parts.Add("Any 'Will Release' Master Game Updates");
        }
        
        return string.Join(',', parts);
    }

    private string GetPublicBiddingRoleDisplayText(PickupSystem pickupSystem, ulong? publicBiddingAlertRoleId)
    {
        if (pickupSystem.Equals(PickupSystem.SecretBidding))
        {
            return "N/A (Bidding is Secret in this league)";
        }

        if (publicBiddingAlertRoleId != null)
        {
            var roleToMention = Context.Guild.Roles.FirstOrDefault(r => r.Id == publicBiddingAlertRoleId);
            if (roleToMention != null)
            {
                return roleToMention.Mention;
            }
        }

        return "No role is set for alerting on public bids. You may use `/set-public-bid-role` to set up a role to mention with public bid announcements. Make sure that role is mentionable!";
    }
}
