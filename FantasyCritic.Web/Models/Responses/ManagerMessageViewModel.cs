using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class ManagerMessageViewModel
    {
        public ManagerMessageViewModel(ManagerMessage domain)
        {
            MessageID = domain.MessageID;
            MessageText = domain.MessageText;
            IsPublic = domain.IsPublic;
            Timestamp = domain.Timestamp;
        }

        public Guid MessageID { get; }
        public string MessageText { get; }
        public bool IsPublic { get; }
        public Instant Timestamp { get; }
    }
}
