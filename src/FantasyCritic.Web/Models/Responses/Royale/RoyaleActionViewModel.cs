using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleActionViewModel
{
    public RoyaleActionViewModel(RoyaleAction domain, LocalDate currentDate, bool thisPlayerIsViewing)
    {
        Timestamp = domain.Timestamp;
        GameHidden = domain.IsHidden(currentDate);
        ActionType = domain.ActionType;

        if (!GameHidden || thisPlayerIsViewing)
        {
            MasterGame = new MasterGameYearViewModel(domain.MasterGame, currentDate);
            Description = domain.Description;
        }
    }

    public MasterGameYearViewModel? MasterGame { get; }
    public bool GameHidden { get; }
    public string? ActionType { get; }
    public string? Description { get; }
    public Instant Timestamp { get; }
}
