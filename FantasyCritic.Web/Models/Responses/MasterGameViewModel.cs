using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameViewModel
    {
        public MasterGameViewModel(MasterGame masterGame, IClock clock)
        {
            MasterGameID = masterGame.MasterGameID;
            GameName = masterGame.GameName;
            EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
            MinimumReleaseDate = masterGame.MinimumReleaseDate;
            MaximumReleaseDate = masterGame.GetDefiniteMaximumReleaseDate();
            EarlyAccessReleaseDate = masterGame.EarlyAccessReleaseDate;
            InternationalReleaseDate = masterGame.InternationalReleaseDate;
            ReleaseDate = masterGame.ReleaseDate;

            IsReleased = masterGame.IsReleased(clock.GetCurrentInstant());
            CriticScore = masterGame.CriticScore;
            AveragedScore = masterGame.AveragedScore;
            Notes = masterGame.Notes;
            OpenCriticID = masterGame.OpenCriticID;
            SubGames = masterGame.SubGames.Select(x => new MasterGameViewModel(x, clock)).ToList();
            Tags = masterGame.Tags.Select(x => x.Name).ToList();
            BoxartFileName = masterGame.BoxartFileName;
            AddedTimestamp = masterGame.AddedTimestamp;
        }

        public MasterGameViewModel(MasterSubGame masterSubGame, IClock clock)
        {
            MasterGameID = masterSubGame.MasterGameID;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            MinimumReleaseDate = masterSubGame.MinimumReleaseDate;
            MaximumReleaseDate = masterSubGame.GetDefiniteMaximumReleaseDate();
            ReleaseDate = masterSubGame.ReleaseDate;
            IsReleased = masterSubGame.IsReleased(clock.GetCurrentInstant());
            CriticScore = masterSubGame.CriticScore;
            AveragedScore = false;
            OpenCriticID = masterSubGame.OpenCriticID;
            SubGames = null;
        }

        public MasterGameViewModel(MasterGame masterGame, IClock clock, bool error)
            : this(masterGame, clock)
        {
            Error = error;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate MinimumReleaseDate { get; }
        public LocalDate MaximumReleaseDate { get; }
        public LocalDate? EarlyAccessReleaseDate { get; }
        public LocalDate? InternationalReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public bool IsReleased { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public int? OpenCriticID { get; }
        public IReadOnlyList<MasterGameViewModel> SubGames { get; }
        public IReadOnlyList<string> Tags { get; }
        public string Notes { get; }
        public string BoxartFileName { get; }
        public Instant AddedTimestamp { get; }
        public bool Error { get; }
    }
}
