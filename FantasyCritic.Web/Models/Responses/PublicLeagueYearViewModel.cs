using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublicLeagueYearViewModel
    {
        public PublicLeagueYearViewModel(LeagueYear leagueYear)
        {
            LeagueID = leagueYear.League.LeagueID;
            LeagueName = leagueYear.League.LeagueName;
            NumberOfFollowers = leagueYear.League.NumberOfFollowers;
            PlayStatus = leagueYear.PlayStatus.Value;
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public int NumberOfFollowers{ get; }
        public string PlayStatus { get; }
    }
}
