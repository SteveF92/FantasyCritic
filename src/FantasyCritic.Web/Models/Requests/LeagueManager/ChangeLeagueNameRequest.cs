using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

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