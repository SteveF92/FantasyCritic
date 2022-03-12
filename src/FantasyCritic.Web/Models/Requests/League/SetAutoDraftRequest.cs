using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class SetAutoDraftRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public bool AutoDraft { get; set; }
}
