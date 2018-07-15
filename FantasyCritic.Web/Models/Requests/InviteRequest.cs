using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class InviteRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public string InviteEmail { get; set; }
    }
}
