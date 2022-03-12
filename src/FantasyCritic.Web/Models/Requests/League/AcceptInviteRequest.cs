using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class AcceptInviteRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
    }
}
