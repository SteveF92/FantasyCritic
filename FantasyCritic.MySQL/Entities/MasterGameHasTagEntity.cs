using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterGameHasTagEntity
    {
        public MasterGameHasTagEntity()
        {

        }

        public MasterGameHasTagEntity(MasterGame masterGame, MasterGameTag tag)
        {
            MasterGameID = masterGame.MasterGameID;
            TagName = tag.Name;
        }

        public Guid MasterGameID { get; set; }
        public string TagName { get; set; }
        public DateTime TimeAdded { get; set; }
    }
}
