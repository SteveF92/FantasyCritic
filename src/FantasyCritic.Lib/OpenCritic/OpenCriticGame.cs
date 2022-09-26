namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticGame
{
    public OpenCriticGame(int id, string name, decimal? score, LocalDate? releaseDate, bool hasAnyReviews, string? slug)
    {
        ID = id;
        Name = name;
        Score = score;
        ReleaseDate = releaseDate;
        HasAnyReviews = hasAnyReviews;
        Slug = slug;
    }

    public int ID { get; }
    public string Name { get; }
    public decimal? Score { get; }
    public LocalDate? ReleaseDate { get; }
    public bool HasAnyReviews { get; }
    public string? Slug { get; }
}
