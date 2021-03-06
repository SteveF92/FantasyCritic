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
            MinimumReleaseDate = masterGame.MinimumReleaseDate;
            MaximumReleaseDate = masterGame.MaximumReleaseDate;
            EarlyAccessReleaseDate = masterGame.EarlyAccessReleaseDate;
            InternationalReleaseDate = masterGame.InternationalReleaseDate;
            ReleaseDate = masterGame.ReleaseDate;
            OpenCriticID = masterGame.OpenCriticID;
            CriticScore = masterGame.CriticScore;
            Notes = masterGame.Notes;
            BoxartFileName = masterGame.BoxartFileName;

            FirstCriticScoreTimestamp = masterGame.FirstCriticScoreTimestamp;
            DoNotRefreshDate = masterGame.DoNotRefreshDate;
            DoNotRefreshAnything = masterGame.DoNotRefreshAnything;
            EligibilityChanged = masterGame.EligibilityChanged;
        }

        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate MinimumReleaseDate { get; set; }
        public LocalDate? MaximumReleaseDate { get; set; }
        public LocalDate? EarlyAccessReleaseDate { get; set; }
        public LocalDate? InternationalReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public string Notes { get; set; }
        public string BoxartFileName { get; set; }
        public Instant? FirstCriticScoreTimestamp { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public bool EligibilityChanged { get; set; }
        public Instant AddedTimestamp { get; set; }

        public MasterGame ToDomain(IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags)
        {
            return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate, EarlyAccessReleaseDate, InternationalReleaseDate, ReleaseDate, OpenCriticID, CriticScore,
                Notes, BoxartFileName, FirstCriticScoreTimestamp, DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, AddedTimestamp, subGames, tags);
        }
    }
}
