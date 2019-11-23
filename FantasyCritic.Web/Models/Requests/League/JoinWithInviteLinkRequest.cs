using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class JoinWithInviteLinkRequest
    {
        [Required]
        public Guid InviteCode { get; set; }
    }
}
