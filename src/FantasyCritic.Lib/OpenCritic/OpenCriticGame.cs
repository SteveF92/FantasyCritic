namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticGame
{
    public OpenCriticGame(int id, string name, decimal? score, bool hasAnyReviews, string? slug)
    {
        ID = id;
        Name = name;
        Score = score;
        HasAnyReviews = hasAnyReviews;
        Slug = slug;
    }

    public int ID { get; }
    public string Name { get; }
    public decimal? Score { get; }
    public bool HasAnyReviews { get; }
    public string? Slug { get; }
}
