using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League.Trades;

public class TradeVoteRequest
{
    [Required]
    public Guid TradeID { get; set; }
    [Required]
    public bool Approved { get; set; }
    public string Comment { get; set; }
}