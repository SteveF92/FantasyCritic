using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    internal class SetDraftOrderEntity
    {
        public SetDraftOrderEntity(Guid publisherID, int draftPosition)
        {
            PublisherID = publisherID;
            DraftPosition = draftPosition;
        }

        public Guid PublisherID { get; }
        public int DraftPosition { get; }
    }
}
