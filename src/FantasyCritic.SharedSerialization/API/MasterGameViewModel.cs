using System.Runtime.InteropServices;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.SharedSerialization.API;

public class MasterGameViewModel
{
    public MasterGameViewModel()
    {
        
    }

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
        ReleasingToday = masterGame.ReleaseDate == currentDate;

        DoNotRefreshDate = masterGame.DoNotRefreshDate;
        DoNotRefreshAnything = masterGame.DoNotRefreshAnything;
        UseSimpleEligibility = masterGame.UseSimpleEligibility;
        DelayContention = masterGame.DelayContention;
        ShowNote = masterGame.ShowNote;

        CriticScore = masterGame.CriticScore;
        AveragedScore = masterGame.AveragedScore;
        Notes = masterGame.Notes;
        OpenCriticID = masterGame.OpenCriticID;
        OpenCriticSlug = masterGame.OpenCriticSlug;
        GGToken = masterGame.GGToken;
        GGSlug = masterGame.GGSlug;
        SubGames = masterGame.SubGames.Select(x => new MasterGameViewModel(x, currentDate)).ToList();
        Tags = masterGame.Tags.Select(x => x.Name).ToList();
        BoxartFileName = masterGame.BoxartFileName;
        GGCoverArtFileName = masterGame.GGCoverArtFileName;
        AddedTimestamp = masterGame.AddedTimestamp;
        AddedByUser = new MinimalFantasyCriticUserViewModel(masterGame.AddedByUser);

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
        ReleasingToday = masterSubGame.ReleaseDate == currentDate;
        CriticScore = masterSubGame.CriticScore;
        AveragedScore = false;
        OpenCriticID = masterSubGame.OpenCriticID;
        SubGames = null;
        AddedByUser = null;
        Tags = new List<string>();
    }

    public Guid MasterGameID { get; init; }
    public string GameName { get; init; } = null!;
    public string EstimatedReleaseDate { get; init; } = null!;
    public LocalDate MinimumReleaseDate { get; init; }
    public LocalDate MaximumReleaseDate { get; init; }
    public LocalDate? EarlyAccessReleaseDate { get; init; }
    public LocalDate? InternationalReleaseDate { get; init; }
    public LocalDate? AnnouncementDate { get; init; }
    public LocalDate? ReleaseDate { get; init; }
    public bool IsReleased { get; init; }
    public bool ReleasingToday { get; init; }
    public bool DoNotRefreshDate { get; init; }
    public bool DoNotRefreshAnything { get; init; }
    public bool UseSimpleEligibility { get; init; }
    public bool DelayContention { get; init; }
    public bool ShowNote { get; init; }
    public decimal? CriticScore { get; init; }
    public bool AveragedScore { get; init; }
    public int? OpenCriticID { get; init; }
    public string? OpenCriticSlug { get; init; }
    public string? GGToken { get; init; }
    public string? GGSlug { get; init; }
    public IReadOnlyList<MasterGameViewModel>? SubGames { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = null!;
    public string? Notes { get; init; }
    public string? BoxartFileName { get; init; }
    public string? GGCoverArtFileName { get; init; }
    public Instant AddedTimestamp { get; init; }
    public MinimalFantasyCriticUserViewModel AddedByUser { get; init; }
    public bool Error { get; init; }
    public int NumberOutstandingCorrections { get; init; }

    public MasterGame ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        List<MasterGameTag> tags = new List<MasterGameTag>();
        foreach (var tag in Tags)
        {
            if (tagDictionary.TryGetValue(tag, out var masterGameTag))
            {
                tags.Add(masterGameTag);
            }
        }

        var addedByUser = new VeryMinimalFantasyCriticUser(AddedByUser.UserID, AddedByUser.DisplayName);

        return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate, EarlyAccessReleaseDate, InternationalReleaseDate,
            AnnouncementDate, ReleaseDate, OpenCriticID, GGToken, GGSlug, CriticScore, CriticScore.HasValue, OpenCriticSlug, Notes, BoxartFileName, GGCoverArtFileName,  AddedTimestamp, DoNotRefreshDate,
            DoNotRefreshAnything, UseSimpleEligibility, DelayContention, ShowNote, AddedTimestamp, addedByUser, new List<MasterSubGame>(), tags);
    }
}
