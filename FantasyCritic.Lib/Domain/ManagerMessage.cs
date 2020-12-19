using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class ManagerMessage
    {
        public ManagerMessage(Guid messageID, string messageText, bool isPublic, Instant timestamp)
        {
            MessageID = messageID;
            MessageText = messageText;
            IsPublic = isPublic;
            Timestamp = timestamp;
        }

        public Guid MessageID { get; }
        public string MessageText { get; }
        public bool IsPublic { get; }
        public Instant Timestamp { get; }
    }
}
