using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameChangeRequestViewModel
    {
        public MasterGameChangeRequestViewModel(MasterGameChangeRequest domain, LocalDate currentDate)
        {
            RequestID = domain.RequestID;
            RequesterDisplayName = domain.User.UserName;
            MasterGame = new MasterGameViewModel(domain.MasterGame, currentDate);

            RequestNote = domain.RequestNote;
            OpenCriticID = domain.OpenCriticID;

            Answered = domain.Answered;
            ResponseNote = domain.ResponseNote;
            ResponseTimestamp = domain.ResponseTimestamp;
            Hidden = domain.Hidden;
        }

        public Guid RequestID { get; }
        public string RequesterDisplayName { get; }
        public string RequestNote { get; }
        public int? OpenCriticID { get; }
        public MasterGameViewModel MasterGame { get; }

        //Response
        public bool Answered { get; }
        public string ResponseNote { get; }
        public Instant? ResponseTimestamp { get; }
        public bool Hidden { get; }
    }
}
