using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameRequestViewModel
    {
        public MasterGameRequestViewModel(MasterGameRequest domain, LocalDate currentDate)
        {
            RequestID = domain.RequestID;
            RequesterDisplayName = domain.User.DisplayName;
            GameName = domain.GameName;
            ReleaseDate = domain.ReleaseDate;
            EstimatedReleaseDate = domain.EstimatedReleaseDate;
            SteamID = domain.SteamID;
            OpenCriticID = domain.OpenCriticID;

            Answered = domain.Answered;
            ResponseNote = domain.ResponseNote;
            ResponseTimestamp = domain.ResponseTimestamp;
            if (domain.MasterGame.HasValue)
            {
                MasterGame = new MasterGameViewModel(domain.MasterGame.Value, currentDate);
            }
            Hidden = domain.Hidden;
            RequestNote = domain.RequestNote;
        }

        public Guid RequestID { get; }
        public string RequesterDisplayName { get; }
        public string GameName { get; }
        public LocalDate? ReleaseDate { get; }
        public string EstimatedReleaseDate { get; }
        public int? SteamID { get; }
        public int? OpenCriticID { get; }
        public string RequestNote { get; }

        //Response
        public bool Answered { get; }
        public string ResponseNote { get; }
        public Instant? ResponseTimestamp { get; }
        public MasterGameViewModel MasterGame { get; }
        public bool Hidden { get; }
    }
}
