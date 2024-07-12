using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities;

internal class RoyaleYearQuarterEntity
{
    public int Year { get; set; }
    public int Quarter { get; set; }
    public bool OpenForPlay { get; set; }
    public bool Finished { get; set; }
    public Guid? WinningUser { get; set; }

    public string? WinningUserDisplayName { get; set; }

    public RoyaleYearQuarter ToDomain()
    {
        VeryMinimalFantasyCriticUser? winningUser = null;
        if (WinningUser.HasValue)
        {
            winningUser = new VeryMinimalFantasyCriticUser(WinningUser.Value, WinningUserDisplayName!);
        }

        return new RoyaleYearQuarter(new YearQuarter(Year, Quarter), OpenForPlay, Finished, winningUser);
    }
}
