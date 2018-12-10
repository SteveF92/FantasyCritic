using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.EmailTemplates
{
    public class LeagueInviteModel
    {
        public LeagueInviteModel(League league, string baseURL)
        {
            League = league;
            Link = baseURL;
        }

        public League League { get; }
        public string Link { get; }
    }
}
