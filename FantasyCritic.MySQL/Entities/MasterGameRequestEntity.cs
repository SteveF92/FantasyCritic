using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterGameRequestEntity
    {
        public MasterGameRequestEntity(MasterGameRequest domain)
        {
            RequestID = domain.RequestID;
            UserID = domain.User.UserID;
            RequestTimestamp = domain.RequestTimestamp.ToDateTimeUtc();
            RequestNote = domain.RequestNote;

            GameName = domain.GameName;
            SteamID = domain.SteamID;
            OpenCriticID = domain.OpenCriticID;
            EstimatedReleaseDate = domain.EstimatedReleaseDate;
            if (domain.EligibilityLevel.HasValue)
            {
                EligibilityLevel = domain.EligibilityLevel.Value.Level;
            }
            YearlyInstallment = domain.YearlyInstallment;
            EarlyAccess = domain.EarlyAccess;

            Answered = domain.Answered;
            ResponseTimestamp = domain.ResponseTimestamp?.ToDateTimeUtc();
            ResponseNote = domain.ResponseNote;

            if (domain.MasterGame.HasValue)
            {
                MasterGameID = domain.MasterGame.Value.MasterGameID;
            }
        }

        //Request
        public Guid RequestID { get; set; }
        public Guid UserID { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string RequestNote { get; set; }

        //Game Details
        public string GameName { get; set; }
        public int? SteamID { get; set; }
        public int? OpenCriticID { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public int? EligibilityLevel { get; set; }
        public bool? YearlyInstallment { get; set; }
        public bool? EarlyAccess { get; set; }

        //Response
        public bool Answered { get; set; }
        public DateTime? ResponseTimestamp { get; set; }
        public string ResponseNote { get; set; }
        public Guid? MasterGameID { get; set; }

        public MasterGameRequest ToDomain(FantasyCriticUser user, Maybe<EligibilityLevel> eligibilityLevel, Maybe<MasterGame> masterGame)
        {
            Instant requestTimestamp = LocalDateTime.FromDateTime(RequestTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            Instant? responseTimestamp = null;
            if (ResponseTimestamp.HasValue)
            {
                responseTimestamp = LocalDateTime.FromDateTime(ResponseTimestamp.Value).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            }

            return new MasterGameRequest(RequestID, user, requestTimestamp, RequestNote, GameName, SteamID, OpenCriticID, EstimatedReleaseDate, eligibilityLevel,
                YearlyInstallment, EarlyAccess, Answered, responseTimestamp, ResponseNote, masterGame);
        }
    }
}
