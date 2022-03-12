using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class CompleteMasterGameChangeRequestRequest
    {
        [Required]
        public Guid RequestID { get; set; }
        [Required]
        public string ResponseNote { get; set; }
    }
}
