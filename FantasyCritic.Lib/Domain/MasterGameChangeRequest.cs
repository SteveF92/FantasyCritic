using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameChangeRequest
    {
        public MasterGameChangeRequest(Guid requestID, FantasyCriticUser user, Instant requestTimestamp, string requestNote,
            MasterGame masterGame, bool answered, Instant? responseTimestamp, string responseNote, bool hidden)
        {
            RequestID = requestID;
            User = user;
            RequestTimestamp = requestTimestamp;
            RequestNote = requestNote;
            MasterGame = masterGame;
            Answered = answered;
            ResponseTimestamp = responseTimestamp;
            ResponseNote = responseNote;
            Hidden = hidden;
        }

        //Request
        public Guid RequestID { get; }
        public FantasyCriticUser User { get; }
        public Instant RequestTimestamp { get; }
        public string RequestNote { get; }
        public MasterGame MasterGame { get; }


        //Answer
        public bool Answered { get; }
        public Instant? ResponseTimestamp { get; }
        public string ResponseNote { get; }

        public bool Hidden { get; }
    }
}
