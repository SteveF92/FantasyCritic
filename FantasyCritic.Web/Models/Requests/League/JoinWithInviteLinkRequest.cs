using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class JoinWithInviteLinkRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public Guid InviteCode { get; set; }
    }
}
