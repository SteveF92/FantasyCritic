using FantasyCritic.Web.Models.Responses;
using FantasyCritic.Web.Models.Responses.Conferences;
using FantasyCritic.Web.Models.Responses.Royale;

namespace FantasyCritic.Web.Models.Responses.Combined;

public record HomePageDataViewModel(
    List<LeagueWithStatusViewModel> MyLeagues,
    List<CompleteLeagueInviteViewModel> MyInvites,
    List<MinimalConferenceViewModel> MyConferences,
    TopBidsAndDropsSetViewModel? TopBidsAndDrops,
    GameNewsViewModel MyGameNews,
    List<PublicLeagueYearViewModel> PublicLeagues,
    RoyaleYearQuarterViewModel ActiveRoyaleQuarter,
    Guid? UserRoyalePublisherID);
