using System;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class DropGameRequestViewModel
    {
        public DropGameRequestViewModel(DropRequest dropRequest, LocalDate currentDate)
        {
            DropRequestID = dropRequest.DropRequestID;
            Timestamp = dropRequest.Timestamp.ToDateTimeUtc();
            Successful = dropRequest.Successful;
            MasterGame = new MasterGameViewModel(dropRequest.MasterGame, currentDate);
        }

        public Guid DropRequestID { get; }
        public DateTime Timestamp { get; }
        public bool? Successful { get; }
        public MasterGameViewModel MasterGame { get; }
    }
}
