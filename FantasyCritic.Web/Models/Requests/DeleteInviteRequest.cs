using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class DeleteInviteRequest
    {
        [Required]
        public Guid InviteID { get; set; }
    }
}
