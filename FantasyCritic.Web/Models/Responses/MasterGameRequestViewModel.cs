using System;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameRequestViewModel
    {
        public MasterGameRequestViewModel(MasterGameRequest domain)
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
        }

        public Guid RequestID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public int? SteamID { get; }
        public int? OpenCriticID { get; }
        public int? EligibilityLevel { get; }
        public bool? YearlyInstallment { get; }
        public bool? EarlyAccess { get; }
    }
}
