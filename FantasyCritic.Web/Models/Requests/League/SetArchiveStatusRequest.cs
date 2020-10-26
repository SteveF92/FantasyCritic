using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class SetArchiveStatusRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public bool Archive { get; set; }
    }
}
