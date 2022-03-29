namespace FantasyCritic.Lib.Domain;

public class MasterGame : IEquatable<MasterGame>
{
    public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate,
        LocalDate? earlyAccessReleaseDate, LocalDate? internationalReleaseDate, LocalDate? announcementDate, LocalDate? releaseDate, int? openCriticID, string? ggToken, decimal? criticScore,
        string notes, string boxartFileName, string ggCoverArtFileName, Instant? firstCriticScoreTimestamp, bool doNotRefreshDate,
        bool doNotRefreshAnything, bool eligibilityChanged, bool delayContention, Instant addedTimestamp, IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags)
    {
        MasterGameID = masterGameID;
        GameName = gameName;
        EstimatedReleaseDate = estimatedReleaseDate;
        MinimumReleaseDate = minimumReleaseDate;
        MaximumReleaseDate = maximumReleaseDate;
        EarlyAccessReleaseDate = earlyAccessReleaseDate;
        InternationalReleaseDate = internationalReleaseDate;
        AnnouncementDate = announcementDate;
        ReleaseDate = releaseDate;
        OpenCriticID = openCriticID;
        GGToken = ggToken;
        RawCriticScore = criticScore;
        Notes = notes;
        BoxartFileName = boxartFileName;
        GGCoverArtFileName = ggCoverArtFileName;
        FirstCriticScoreTimestamp = firstCriticScoreTimestamp;
        DoNotRefreshDate = doNotRefreshDate;
        DelayContention = delayContention;
        DoNotRefreshAnything = doNotRefreshAnything;
        EligibilityChanged = eligibilityChanged;
        AddedTimestamp = addedTimestamp;
        SubGames = subGames.ToList();
        Tags = tags.ToList();
    }

    public Guid MasterGameID { get; }
    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MinimumReleaseDate { get; }
    public LocalDate? MaximumReleaseDate { get; }
    public LocalDate? EarlyAccessReleaseDate { get; }
    public LocalDate? InternationalReleaseDate { get; }
    public LocalDate? AnnouncementDate { get; }
    public LocalDate? ReleaseDate { get; }
    public int? OpenCriticID { get; }
    public string? GGToken { get; }

    public string BoxartFileName { get; }
    public string GGCoverArtFileName { get; }
    public Instant? FirstCriticScoreTimestamp { get; }
    public bool DoNotRefreshDate { get; }
    public bool DelayContention { get; }
    public bool DoNotRefreshAnything { get; }
    public bool EligibilityChanged { get; }
    public Instant AddedTimestamp { get; }
    public IReadOnlyList<MasterSubGame> SubGames { get; }
    public IReadOnlyList<MasterGameTag> Tags { get; }

    public LocalDate GetDefiniteMaximumReleaseDate() => MaximumReleaseDate ?? LocalDate.MaxIsoValue;

    public decimal? RawCriticScore { get; }
    public decimal? CriticScore
    {
        get
        {
            if (RawCriticScore.HasValue)
            {
                return RawCriticScore;
            }

            if (!SubGames.Any(x => x.CriticScore.HasValue))
            {
                return null;
            }

            decimal average = SubGames.Where(x => x.CriticScore.HasValue).Average(x => x.CriticScore.Value);
            return average;
        }
    }

    public bool AveragedScore
    {
        get
        {
            if (RawCriticScore.HasValue)
            {
                return false;
            }

            if (!SubGames.Any(x => x.CriticScore.HasValue))
            {
                return false;
            }

            return true;
        }
    }

    public string Notes { get; }

    public string GetReleaseDateString()
    {
        if (ReleaseDate.HasValue)
        {
            return ReleaseDate.Value.ToString();
        }
        return EstimatedReleaseDate + " (Estimated)";
    }

    public bool IsReleased(LocalDate currentDate)
    {
        if (SubGames.Any(x => x.IsReleased(currentDate)))
        {
            return true;
        }

        if (!ReleaseDate.HasValue)
        {
            return false;
        }

        if (currentDate >= ReleaseDate.Value)
        {
            return true;
        }

        return false;
    }

    public override string ToString() => GameName;

    public bool Equals(MasterGame other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return MasterGameID.Equals(other.MasterGameID);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MasterGame)obj);
    }

    public override int GetHashCode()
    {
        return MasterGameID.GetHashCode();
    }
}
