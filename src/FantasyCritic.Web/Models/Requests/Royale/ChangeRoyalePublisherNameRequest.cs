using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Royale;

public class ChangeRoyalePublisherNameRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public string PublisherName { get; set; }
}