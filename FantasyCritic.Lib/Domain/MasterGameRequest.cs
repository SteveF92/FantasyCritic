using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameRequest
    {
        public MasterGameRequest(Guid requestID, FantasyCriticUser user, Instant requestTimestamp, string requestNote, 
            string gameName, int? steamID, int? openCriticID, string estimatedReleaseDate, Maybe<EligibilityLevel> eligibilityLevel,
            bool? yearlyInstallment, bool? earlyAccess, bool? freeToPlay, bool? releasedInternationally, bool? expansionPack,
            bool answered, Instant? responseTimestamp, string responseNote, Maybe<MasterGame> masterGame, bool hidden)
        {
            RequestID = requestID;
            User = user;
            RequestTimestamp = requestTimestamp;
            RequestNote = requestNote;
            GameName = gameName;
            SteamID = steamID;
            OpenCriticID = openCriticID;
            EstimatedReleaseDate = estimatedReleaseDate;
            EligibilityLevel = eligibilityLevel;
            YearlyInstallment = yearlyInstallment;
            EarlyAccess = earlyAccess;
            FreeToPlay = freeToPlay;
            ReleasedInternationally = releasedInternationally;
            ExpansionPack = expansionPack;
            Answered = answered;
            ResponseTimestamp = responseTimestamp;
            ResponseNote = responseNote;
            MasterGame = masterGame;
            Hidden = hidden;
        }

        //Request
        public Guid RequestID { get; }
        public FantasyCriticUser User { get; }
        public Instant RequestTimestamp { get; }
        public string RequestNote { get; }

        //Game Details
        public string GameName { get; }
        public int? SteamID { get; }
        public int? OpenCriticID { get; }
        public string EstimatedReleaseDate { get; }
        public Maybe<EligibilityLevel> EligibilityLevel { get; }
        public bool? YearlyInstallment { get; }
        public bool? EarlyAccess { get; }
        public bool? FreeToPlay { get; }
        public bool? ReleasedInternationally { get; }
        public bool? ExpansionPack { get; }

        //Answer
        public bool Answered { get; }
        public Instant? ResponseTimestamp { get; }
        public string ResponseNote { get; }
        public Maybe<MasterGame> MasterGame { get; }

        public bool Hidden { get; }
    }
}
