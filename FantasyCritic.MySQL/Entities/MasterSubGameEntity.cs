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
        public DateTime? SortableEstimatedReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public int MinimumReleaseDate { get; set; }

        public MasterSubGame ToDomain()
        {
            LocalDate? releaseDate = null;
            if (ReleaseDate.HasValue)
            {
                releaseDate = LocalDate.FromDateTime(ReleaseDate.Value);
            }

            LocalDate sortableEstimatedReleaseDate = LocalDate.MaxIsoValue;
            if (SortableEstimatedReleaseDate.HasValue)
            {
                sortableEstimatedReleaseDate = LocalDate.FromDateTime(SortableEstimatedReleaseDate.Value);
            }

            return new MasterSubGame(MasterSubGameID, MasterGameID, GameName, EstimatedReleaseDate, sortableEstimatedReleaseDate, 
                releaseDate, OpenCriticID, CriticScore, MinimumReleaseDate);
        }
    }
}
