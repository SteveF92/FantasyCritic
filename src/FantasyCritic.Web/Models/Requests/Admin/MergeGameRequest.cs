using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin;

public class MergeGameRequest
{
    [Required]
    public Guid RemoveMasterGameID { get; set; }
    [Required]
    public Guid MergeIntoMasterGameID { get; set; }
}