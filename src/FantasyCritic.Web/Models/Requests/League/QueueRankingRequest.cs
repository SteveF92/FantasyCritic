using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League;

public class QueueRankingRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public List<Guid> QueueRanks { get; set; }
}