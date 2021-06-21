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
            RequestTimestamp = domain.RequestTimestamp;
            RequestNote = domain.RequestNote;
            MasterGameID = domain.MasterGame.MasterGameID;
            OpenCriticID = domain.OpenCriticID;

            Answered = domain.Answered;
            ResponseTimestamp = domain.ResponseTimestamp;
            ResponseNote = domain.ResponseNote;

            Hidden = domain.Hidden;
        }

        //Request
        public Guid RequestID { get; set; }
        public Guid UserID { get; set; }
        public Instant RequestTimestamp { get; set; }
        public string RequestNote { get; set; }
        public Guid MasterGameID { get; set; }
        public int? OpenCriticID { get; set; }

        //Response
        public bool Answered { get; set; }
        public Instant? ResponseTimestamp { get; set; }
        public string ResponseNote { get; set; }
        
        public bool Hidden { get; set; }

        public MasterGameChangeRequest ToDomain(FantasyCriticUser user, MasterGame masterGame)
        {
            return new MasterGameChangeRequest(RequestID, user, RequestTimestamp, RequestNote, masterGame, OpenCriticID, Answered, ResponseTimestamp, ResponseNote, Hidden);
        }
    }
}
