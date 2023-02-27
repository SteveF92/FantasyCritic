using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Web.Models.Responses;

public class SingleGameNewsViewModel
{
    public SingleGameNewsViewModel(MasterGameYear masterGame, IReadOnlyList<LeagueYearPublisherPair> publishersPairsThatHaveGame, bool userMode, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(masterGame, currentDate);
        MasterGameID = masterGame.MasterGame.MasterGameID;
        GameName = masterGame.MasterGame.GameName;
        EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
        MaximumReleaseDate = masterGame.MasterGame.GetDefiniteMaximumReleaseDate();
        ReleaseDate = masterGame.MasterGame.ReleaseDate;

        if (userMode)
        {
            LeaguePublisherSets = publishersPairsThatHaveGame.Select(x => new SingleGameNewsPublisherViewModel(x)).ToList();
        }
        else
        {
            LeaguePublisherSets = new List<SingleGameNewsPublisherViewModel>()
            {
                new SingleGameNewsPublisherViewModel(masterGame.MasterGame, publishersPairsThatHaveGame)
            };
        }
    }

    public MasterGameYearViewModel MasterGame { get; }
    public Guid MasterGameID { get; }
    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MaximumReleaseDate { get; }
    public LocalDate? ReleaseDate { get; }
    public IReadOnlyList<SingleGameNewsPublisherViewModel> LeaguePublisherSets { get; }
}
