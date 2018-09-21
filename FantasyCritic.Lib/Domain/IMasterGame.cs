using System;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public interface IMasterGame
    {
        Guid MasterGameID { get; }
        string GameName { get; }
        string EstimatedReleaseDate { get; }
        LocalDate? ReleaseDate { get; }
        int? OpenCriticID { get; }
        decimal? CriticScore { get; }
        int MinimumReleaseYear { get; }
    }
}