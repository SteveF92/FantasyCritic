using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin;

public class DeleteLeagueRequest
{
    [Required]
    public Guid LeagueID { get; set; }
}