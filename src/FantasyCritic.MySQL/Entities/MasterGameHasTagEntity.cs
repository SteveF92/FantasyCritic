using System.Runtime.CompilerServices;

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
