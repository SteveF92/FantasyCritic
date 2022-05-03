namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticGame
{
    public OpenCriticGame(int id, string name, decimal? score, LocalDate? releaseDate, bool hasAnyReviews)
    {
        ID = id;
        Name = name;
        Score = score;
        ReleaseDate = releaseDate;
        HasAnyReviews = hasAnyReviews;
    }

    public int ID { get; }
    public string Name { get; }
    public decimal? Score { get; }
    public LocalDate? ReleaseDate { get; }
    public bool HasAnyReviews { get; }
}
