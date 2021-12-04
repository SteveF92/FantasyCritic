using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public record PublisherGameSlotNumberUpdateEntity(Guid PublisherGameID, int SlotNumber);
}
