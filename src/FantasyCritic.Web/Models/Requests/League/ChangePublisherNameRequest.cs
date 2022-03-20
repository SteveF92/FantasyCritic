using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class ChangePublisherNameRequest
{
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public int Year { get; set; }
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public string PublisherName { get; set; }
}
