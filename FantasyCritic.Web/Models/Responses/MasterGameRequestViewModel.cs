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

            Answered = domain.Answered;
            ResponseNote = domain.ResponseNote;
            if (domain.MasterGame.HasValue)
            {
                MasterGame = new MasterGameViewModel(domain.MasterGame.Value, clock);
            }
            Hidden = domain.Hidden;
        }

        public Guid RequestID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public int? SteamID { get; }
        public int? OpenCriticID { get; }
        public int? EligibilityLevel { get; }
        public bool? YearlyInstallment { get; }
        public bool? EarlyAccess { get; }

        //Response
        public bool Answered { get; }
        public string ResponseNote { get; }
        public MasterGameViewModel MasterGame { get; }
        public bool Hidden { get; }
    }
}
