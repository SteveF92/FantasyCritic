using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGame
    {
        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate? releaseDate, decimal criticScore)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            ReleaseDate = releaseDate;
            CriticScore = criticScore;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public decimal CriticScore { get; }
    }
}
