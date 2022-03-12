using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class LinkGameToOpenCriticRequest
    {
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public int OpenCriticID { get; set; }
    }
}
