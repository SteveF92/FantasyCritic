using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class ChangePublisherNameRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public string PublisherName { get; set; }
}