using System;
using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League
{
    public class DropGameRequestDeleteRequest
    {
        [Required]
        public Guid DropRequestID { get; set; }
    }
}
