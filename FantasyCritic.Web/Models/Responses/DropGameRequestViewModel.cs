using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class DropGameRequestViewModel
    {
        public DropGameRequestViewModel(DropRequest dropRequest, IClock clock)
        {
            DropRequestID = dropRequest.DropRequestID;
            Timestamp = dropRequest.Timestamp.ToDateTimeUtc();
            Successful = dropRequest.Successful;
            MasterGame = new MasterGameViewModel(dropRequest.MasterGame, clock);
        }

        public Guid DropRequestID { get; }
        public DateTime Timestamp { get; }
        public bool? Successful { get; }
        public MasterGameViewModel MasterGame { get; }
    }
}
