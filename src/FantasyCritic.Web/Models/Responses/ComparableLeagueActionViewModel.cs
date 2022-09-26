using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;

public class ComparableLeagueActionViewModel
{
    public ComparableLeagueActionViewModel(LeagueAction domain)
    {
        LeagueID = domain.Publisher.LeagueYearKey.LeagueID;
        PublisherID = domain.Publisher.PublisherID;
        PublisherName = domain.Publisher.PublisherName;
        ActionType = domain.ActionType;
        Description = domain.Description;
    }

    public Guid LeagueID { get; }
    public Guid PublisherID { get; }
    public string PublisherName { get; }
    public string ActionType { get; }
    public string Description { get; }
}
