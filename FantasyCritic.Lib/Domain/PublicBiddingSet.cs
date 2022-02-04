using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class PublicBiddingSet
    {
        public PublicBiddingSet(LeagueYear leagueYear, IEnumerable<PublicBiddingMasterGame> masterGames)
        {
            LeagueYear = leagueYear;
            MasterGames = masterGames.ToList();
        }

        public LeagueYear LeagueYear { get; }
        public IReadOnlyList<PublicBiddingMasterGame> MasterGames { get; }
    }
}
