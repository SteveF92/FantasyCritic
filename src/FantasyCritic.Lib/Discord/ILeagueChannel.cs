using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.Lib.Discord;

public interface ILeagueChannel : IDiscordChannel
{
    Guid LeagueID { get; }
    bool ShowPickedGameNews { get; }
    bool ShowEligibleGameNews { get; }
    bool ShowIneligibleGameNews { get; }
    NotableMissSetting NotableMissSetting { get; }
    ulong? BidAlertRoleID { get; }
    int? Year { get; }
    ulong? BotAdminRoleID { get; }
}
