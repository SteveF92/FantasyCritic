namespace FantasyCritic.Web.Models.Responses;

public class MasterGameViewModel
{
    public MasterGameViewModel(MasterGame masterGame, LocalDate currentDate, bool error = false, int numberOutstandingCorrections = 0)
    {
        MasterGameID = masterGame.MasterGameID;
        GameName = masterGame.GameName;
        EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
        MinimumReleaseDate = masterGame.MinimumReleaseDate;
        MaximumReleaseDate = masterGame.GetDefiniteMaximumReleaseDate();
        EarlyAccessReleaseDate = masterGame.EarlyAccessReleaseDate;
        InternationalReleaseDate = masterGame.InternationalReleaseDate;
        AnnouncementDate = masterGame.AnnouncementDate;
        ReleaseDate = masterGame.ReleaseDate;

        IsReleased = masterGame.IsReleased(currentDate);
        DelayContention = masterGame.DelayContention;
        CriticScore = masterGame.CriticScore;
        AveragedScore = masterGame.AveragedScore;
        Notes = masterGame.Notes;
        OpenCriticID = masterGame.OpenCriticID;
        GGToken = masterGame.GGToken.GetValueOrDefault();
        SubGames = masterGame.SubGames.Select(x => new MasterGameViewModel(x, currentDate)).ToList();
        Tags = masterGame.Tags.Select(x => x.Name).ToList();
        BoxartFileName = masterGame.BoxartFileName;
        GGCoverArtFileName = masterGame.GGCoverArtFileName;
        AddedTimestamp = masterGame.AddedTimestamp;

        Error = error;
        NumberOutstandingCorrections = numberOutstandingCorrections;
    }

    public MasterGameViewModel(MasterSubGame masterSubGame, LocalDate currentDate)
    {
        MasterGameID = masterSubGame.MasterGameID;
        GameName = masterSubGame.GameName;
        EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
        MinimumReleaseDate = masterSubGame.MinimumReleaseDate;
        MaximumReleaseDate = masterSubGame.GetDefiniteMaximumReleaseDate();
        ReleaseDate = masterSubGame.ReleaseDate;
        IsReleased = masterSubGame.IsReleased(currentDate);
        CriticScore = masterSubGame.CriticScore;
        AveragedScore = false;
        OpenCriticID = masterSubGame.OpenCriticID;
        SubGames = null;
    }

    public Guid MasterGameID { get; }
    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MinimumReleaseDate { get; }
    public LocalDate MaximumReleaseDate { get; }
    public LocalDate? EarlyAccessReleaseDate { get; }
    public LocalDate? InternationalReleaseDate { get; }
    public LocalDate? AnnouncementDate { get; }
    public LocalDate? ReleaseDate { get; }
    public bool IsReleased { get; }
    public bool DelayContention { get; }
    public decimal? CriticScore { get; }
    public bool AveragedScore { get; }
    public int? OpenCriticID { get; }
    public string GGToken { get; }
    public IReadOnlyList<MasterGameViewModel> SubGames { get; }
    public IReadOnlyList<string> Tags { get; }
    public string Notes { get; }
    public string BoxartFileName { get; }
    public string GGCoverArtFileName { get; }
    public Instant AddedTimestamp { get; }
    public bool Error { get; }
    public int NumberOutstandingCorrections { get; }
}
