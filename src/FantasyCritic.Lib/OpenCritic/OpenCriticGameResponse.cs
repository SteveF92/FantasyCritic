namespace FantasyCritic.Lib.OpenCritic;

public class OpenCriticGameResponse
{
    public string? Name { get; set; }
    public string? FirstReleaseDate { get; set; }
    public decimal? TopCriticScore { get; set; }
    public decimal? AverageScore { get; set; }
    public int? NumReviews { get; set; }
    public string? Url { get; set; }
}
