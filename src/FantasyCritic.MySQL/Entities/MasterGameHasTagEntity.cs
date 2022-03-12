using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

[assembly: InternalsVisibleTo("FantasyCritic.BetaSync")]
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
        public Instant TimeAdded { get; set; }
    }
}
