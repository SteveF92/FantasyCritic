using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests.LeagueManager
{
    public class PromoteNewLeagueManagerRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public Guid NewManagerUserID { get; set; }
    }
}
