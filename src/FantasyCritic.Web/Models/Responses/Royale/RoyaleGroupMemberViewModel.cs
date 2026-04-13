using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleGroupMemberViewModel
{
    public RoyaleGroupMemberViewModel(RoyaleGroupMemberDisplayRow row)
    {
        UserID = row.User.UserID;
        DisplayName = row.User.DisplayName;
        HasPublisher = row.Publisher is not null;
        PublisherID = row.Publisher?.PublisherID;
        PublisherName = row.Publisher?.PublisherName;
        TotalFantasyPoints = row.Publisher?.GetTotalFantasyPoints();
    }

    public Guid UserID { get; }
    public string DisplayName { get; }
    public bool HasPublisher { get; }
    public Guid? PublisherID { get; }
    public string? PublisherName { get; }
    public decimal? TotalFantasyPoints { get; }
}
