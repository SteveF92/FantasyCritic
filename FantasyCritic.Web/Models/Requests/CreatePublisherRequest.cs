using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Requests
{
    public class CreatePublisherRequest
    {
        [Required]
        public Guid LeagueID { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string PublisherName { get; set; }
    }
}
