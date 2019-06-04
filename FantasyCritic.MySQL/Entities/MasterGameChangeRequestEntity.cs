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
    internal class MasterGameChangeRequestEntity
    {
        public MasterGameChangeRequestEntity()
        {

        }
        
        public MasterGameChangeRequestEntity(MasterGameChangeRequest domain)
        {
            RequestID = domain.RequestID;
            UserID = domain.User.UserID;
            RequestTimestamp = domain.RequestTimestamp.ToDateTimeUtc();
            RequestNote = domain.RequestNote;
            MasterGameID = domain.MasterGame.MasterGameID;

            Answered = domain.Answered;
            ResponseTimestamp = domain.ResponseTimestamp?.ToDateTimeUtc();
            ResponseNote = domain.ResponseNote;

            Hidden = domain.Hidden;
        }

        //Request
        public Guid RequestID { get; set; }
        public Guid UserID { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string RequestNote { get; set; }
        public Guid MasterGameID { get; set; }

        //Response
        public bool Answered { get; set; }
        public DateTime? ResponseTimestamp { get; set; }
        public string ResponseNote { get; set; }
        
        public bool Hidden { get; set; }

        public MasterGameChangeRequest ToDomain(FantasyCriticUser user, MasterGame masterGame)
        {
            Instant requestTimestamp = LocalDateTime.FromDateTime(RequestTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            Instant? responseTimestamp = null;
            if (ResponseTimestamp.HasValue)
            {
                responseTimestamp = LocalDateTime.FromDateTime(ResponseTimestamp.Value).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            }

            return new MasterGameChangeRequest(RequestID, user, requestTimestamp, RequestNote, masterGame, Answered, responseTimestamp, ResponseNote, Hidden);
        }
    }
}
