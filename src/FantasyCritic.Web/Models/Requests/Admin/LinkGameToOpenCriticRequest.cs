using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class LinkGameToGGRequest
    {
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public string GGToken { get; set; }
    }
}
