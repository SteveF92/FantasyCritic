using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

public class LeagueYearWinnerUpdateEntity
{
    public LeagueYearWinnerUpdateEntity(KeyValuePair<LeagueYearKey, FantasyCriticUser> keyValuePair)
    {
        LeagueID = keyValuePair.Key.LeagueID;
        Year = keyValuePair.Key.Year;
        WinningUserID = keyValuePair.Value.Id;
    }

    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid WinningUserID { get; set; }
}
