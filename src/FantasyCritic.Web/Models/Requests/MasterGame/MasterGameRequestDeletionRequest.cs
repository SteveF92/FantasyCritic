using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.MasterGame;

public class MasterGameRequestDeletionRequest
{
    [Required]
    public Guid RequestID { get; set; }
}
