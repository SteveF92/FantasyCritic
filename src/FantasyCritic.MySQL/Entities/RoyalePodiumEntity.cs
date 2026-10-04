using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.MySQL.Entities;

internal class RoyalePodiumEntity
{
    public Guid PublisherID { get; set; }
    public Guid UserID { get; set; }
    public string PlayerName { get; set; } = null!;
    public string PublisherName { get; set; } = null!;
    public string? PublisherIcon { get; set; }
    public int Ranking { get; set; }
    public decimal TotalFantasyPoints { get; set; }

    public RoyalePodiumEntry ToDomain()
    {
        var user = new VeryMinimalFantasyCriticUser(UserID, PlayerName);
        return new RoyalePodiumEntry(PublisherID, user, PublisherName, PublisherIcon, TotalFantasyPoints, Ranking);
    }
}
