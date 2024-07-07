using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;

public interface ICombinedDataRepo
{
    Task<BasicData> GetBasicData();
    Task<HomePageData> GetHomePageData(FantasyCriticUser currentUser);
    Task<LeagueYear?> GetLeagueYear(Guid leagueID, int year);
    Task<LeagueYearWithUserStatus?> GetLeagueYearWithUserStatus(Guid leagueID, int year);
    Task<LeagueYearWithSupplementalDataFromRepo?> GetLeagueYearWithSupplementalData(Guid leagueID, int year, FantasyCriticUser? currentUser);
    Task<ConferenceYearData?> GetConferenceYearData(Guid conferenceID, int year);
    Task<IReadOnlyList<LeagueYear>> GetLeagueYearsForConferenceYear(Guid conferenceID, int year);
}
