using System;
using System.Collections.Generic;
using System.Text;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterSubGame
    {
        public MasterSubGame(Guid masterSubGameID, Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate minimumReleaseDate, 
            LocalDate? maximumReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore)
        {
            MasterSubGameID = masterSubGameID;
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            MinimumReleaseDate = minimumReleaseDate;
            MaximumReleaseDate = maximumReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            CriticScore = criticScore;
        }

        public Guid MasterSubGameID { get; }
        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate MinimumReleaseDate { get; }
        public LocalDate? MaximumReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public int? OpenCriticID { get; }
        public decimal? CriticScore { get; }

        public LocalDate GetDefiniteMaximumReleaseDate() => MaximumReleaseDate ?? LocalDate.MaxIsoValue;

        public bool IsReleased(Instant timeToCheck)
        {
            if (!ReleaseDate.HasValue)
            {
                return false;
            }

            LocalDate currentDate = timeToCheck.InZone(TimeExtensions.EasternTimeZone).LocalDateTime.Date;
            if (currentDate >= ReleaseDate.Value)
            {
                return true;
            }

            return false;
        }

        public override string ToString() => GameName;
    }
}
