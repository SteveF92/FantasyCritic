using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class CompleteMasterGameRequestRequest
    {
        [Required]
        public Guid RequestID { get; set; }
        [Required]
        public string ResponseNote { get; set; }
        public Guid? MasterGameID { get; set; }
    }
}
