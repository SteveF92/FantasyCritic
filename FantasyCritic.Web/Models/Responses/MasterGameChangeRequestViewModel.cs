using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameChangeRequestViewModel
    {
        public MasterGameChangeRequestViewModel(MasterGameChangeRequest domain, IClock clock)
        {
            RequestID = domain.RequestID;
            RequesterDisplayName = domain.User.DisplayName;
            MasterGame = new MasterGameViewModel(domain.MasterGame, clock);

            Answered = domain.Answered;
            ResponseNote = domain.ResponseNote;
            ResponseTimestamp = domain.ResponseTimestamp;
            Hidden = domain.Hidden;
            RequestNote = domain.RequestNote;
        }

        public Guid RequestID { get; }
        public string RequesterDisplayName { get; }
        public string RequestNote { get; }
        public MasterGameViewModel MasterGame { get; }

        //Response
        public bool Answered { get; }
        public string ResponseNote { get; }
        public Instant? ResponseTimestamp { get; }
        public bool Hidden { get; }
    }
}
