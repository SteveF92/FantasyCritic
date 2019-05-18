using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameRequestViewModel
    {
        public MasterGameRequestViewModel(MasterGameRequest domain, IClock clock)
        {
            RequestID = domain.RequestID;
            RequesterDisplayName = domain.User.DisplayName;
            GameName = domain.GameName;
            EstimatedReleaseDate = domain.EstimatedReleaseDate;
            SteamID = domain.SteamID;
            OpenCriticID = domain.OpenCriticID;
            if (domain.EligibilityLevel.HasValue)
            {
                EligibilityLevel = domain.EligibilityLevel.Value.Level;
            }
            YearlyInstallment = domain.YearlyInstallment;
            EarlyAccess = domain.EarlyAccess;
            FreeToPlay = domain.FreeToPlay;
            ReleasedInternationally = domain.ReleasedInternationally;

            Answered = domain.Answered;
            ResponseNote = domain.ResponseNote;
            ResponseTimestamp = domain.ResponseTimestamp;
            if (domain.MasterGame.HasValue)
            {
                MasterGame = new MasterGameViewModel(domain.MasterGame.Value, clock);
            }
            Hidden = domain.Hidden;
            RequestNote = domain.RequestNote;
        }

        public Guid RequestID { get; }
        public string RequesterDisplayName { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public int? SteamID { get; }
        public int? OpenCriticID { get; }
        public int? EligibilityLevel { get; }
        public bool? YearlyInstallment { get; }
        public bool? EarlyAccess { get; }
        public bool? FreeToPlay { get; }
        public bool? ReleasedInternationally { get; }
        public string RequestNote { get; }

        //Response
        public bool Answered { get; }
        public string ResponseNote { get; }
        public Instant? ResponseTimestamp { get; }
        public MasterGameViewModel MasterGame { get; }
        public bool Hidden { get; }
    }
}
