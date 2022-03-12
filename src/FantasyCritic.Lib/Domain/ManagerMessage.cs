using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class ManagerMessage
    {
        public ManagerMessage(Guid messageID, string messageText, bool isPublic, Instant timestamp, IEnumerable<Guid> dismissedByUserIDs)
        {
            MessageID = messageID;
            MessageText = messageText;
            IsPublic = isPublic;
            Timestamp = timestamp;
            DismissedByUserIDs = dismissedByUserIDs.ToList();
        }

        public Guid MessageID { get; }
        public string MessageText { get; }
        public bool IsPublic { get; }
        public Instant Timestamp { get; }
        public IReadOnlyList<Guid> DismissedByUserIDs { get; }

        public bool IsDismissed(Maybe<FantasyCriticUser> accessingUser)
        {
            if (accessingUser.HasNoValue)
            {
                return false;
            }

            return DismissedByUserIDs.Contains(accessingUser.Value.Id);
        }
    }
}
