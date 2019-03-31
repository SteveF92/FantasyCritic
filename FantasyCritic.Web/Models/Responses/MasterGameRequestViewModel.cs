using System;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameRequestViewModel
    {
        public MasterGameRequestViewModel(MasterGameRequest domain)
        {
            RequestID = domain.RequestID;
            GameName = domain.GameName;
        }

        public Guid RequestID { get; }
        public string GameName { get; }
    }
}
