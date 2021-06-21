using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterSubGameEntity
    {
        public Guid MasterSubGameID { get; set; }
        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate MinimumReleaseDate { get; set; }
        public LocalDate? MaximumReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }

        public MasterSubGame ToDomain()
        {
            return new MasterSubGame(MasterSubGameID, MasterGameID, GameName, EstimatedReleaseDate,
                MinimumReleaseDate, MaximumReleaseDate, ReleaseDate, OpenCriticID, CriticScore);
        }
    }
}
