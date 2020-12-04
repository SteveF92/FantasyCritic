using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterGameHasTagEntity
    {
        public Guid MasterGameID { get; set; }
        public string TagName { get; set; }
        public DateTime TimeAdded { get; set; }
    }
}
