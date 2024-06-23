namespace FantasyCritic.Lib.SharedSerialization.API;

public class MasterSubGameViewModel
{
    public MasterSubGameViewModel()
    {
        
    }

    public MasterSubGameViewModel(MasterSubGame masterSubGame, LocalDate currentDate)
    {
        MasterGameID = masterSubGame.MasterGameID;
        GameName = masterSubGame.GameName;
        EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
        MinimumReleaseDate = masterSubGame.MinimumReleaseDate;
        MaximumReleaseDate = masterSubGame.GetDefiniteMaximumReleaseDate();
        ReleaseDate = masterSubGame.ReleaseDate;
        IsReleased = masterSubGame.IsReleased(currentDate);
        ReleasingToday = masterSubGame.ReleaseDate == currentDate;
        CriticScore = masterSubGame.CriticScore;
        AveragedScore = false;
        OpenCriticID = masterSubGame.OpenCriticID;
    }

    public Guid MasterGameID { get; init; }
    public string GameName { get; init; } = null!;
    public string EstimatedReleaseDate { get; init; } = null!;
    public LocalDate MinimumReleaseDate { get; init; }
    public LocalDate MaximumReleaseDate { get; init; }
    public LocalDate? ReleaseDate { get; init; }
    public bool IsReleased { get; init; }
    public bool ReleasingToday { get; init; }
    public decimal? CriticScore { get; init; }
    public bool AveragedScore { get; init; }
    public int? OpenCriticID { get; init; }
}
