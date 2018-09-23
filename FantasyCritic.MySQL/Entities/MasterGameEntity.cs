using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterGameEntity
    {
        public MasterGameEntity()
        {

        }

        public MasterGameEntity(MasterGame masterGame)
        {
            MasterGameID = masterGame.MasterGameID;
            GameName = masterGame.GameName;
            EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
            ReleaseDate = masterGame.ReleaseDate?.ToDateTimeUnspecified();
            OpenCriticID = masterGame.OpenCriticID;
            CriticScore = masterGame.CriticScore;
            MinimumReleaseYear = masterGame.MinimumReleaseYear;
            EligibilityLevel = masterGame.EligibilityLevel.Level;
        }

        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public int MinimumReleaseYear { get; set; }
        public int EligibilityLevel { get; set; }

        public MasterGame ToDomain(IEnumerable<MasterSubGame> subGames, EligibilityLevel eligibilityLevel)
        {
            LocalDate? releaseDate = null;
            if (ReleaseDate.HasValue)
            {
                releaseDate = LocalDate.FromDateTime(ReleaseDate.Value);
            }

            return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, releaseDate, OpenCriticID, CriticScore, MinimumReleaseYear, eligibilityLevel, subGames.ToList());
        }
    }
}
