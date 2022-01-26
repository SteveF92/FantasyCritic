using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using NodaTime;

[assembly: InternalsVisibleTo("FantasyCritic.BetaSync")]
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
            AnnouncementDate = masterGame.AnnouncementDate;
            ReleaseDate = masterGame.ReleaseDate;
            OpenCriticID = masterGame.OpenCriticID;
            GGToken = masterGame.GGToken.GetValueOrDefault();
            CriticScore = masterGame.CriticScore;
            Notes = masterGame.Notes;
            BoxartFileName = masterGame.BoxartFileName;
            GGCoverArtFileName = masterGame.GGCoverArtFileName;

            FirstCriticScoreTimestamp = masterGame.FirstCriticScoreTimestamp;
            DoNotRefreshDate = masterGame.DoNotRefreshDate;
            DoNotRefreshAnything = masterGame.DoNotRefreshAnything;
            EligibilityChanged = masterGame.EligibilityChanged;
            AddedTimestamp = masterGame.AddedTimestamp;
        }

        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate MinimumReleaseDate { get; set; }
        public LocalDate? MaximumReleaseDate { get; set; }
        public LocalDate? EarlyAccessReleaseDate { get; set; }
        public LocalDate? InternationalReleaseDate { get; set; }
        public LocalDate? AnnouncementDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public string GGToken { get; set; }
        public decimal? CriticScore { get; set; }
        public string Notes { get; set; }
        public string BoxartFileName { get; set; }
        public string GGCoverArtFileName { get; set; }
        public Instant? FirstCriticScoreTimestamp { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public bool EligibilityChanged { get; set; }
        public Instant AddedTimestamp { get; set; }

        public MasterGame ToDomain(IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags)
        {
            return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, MinimumReleaseDate, MaximumReleaseDate, EarlyAccessReleaseDate, InternationalReleaseDate, 
                AnnouncementDate, ReleaseDate, OpenCriticID, GGToken.ToMaybe(), CriticScore,Notes, BoxartFileName, GGCoverArtFileName, FirstCriticScoreTimestamp, 
                DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, AddedTimestamp, subGames, tags);
        }
    }
}
