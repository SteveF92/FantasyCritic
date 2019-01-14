using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class ChangeLeagueOptionsRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public string LeagueName { get; set; }
        [Required]
        public bool PublicLeague { get; set; }
        [Required]
        public bool TestLeague { get; set; }
    }
}
