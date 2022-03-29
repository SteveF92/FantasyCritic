using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class ChangePublisherIconRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    public string? PublisherIcon { get; set; }
}
