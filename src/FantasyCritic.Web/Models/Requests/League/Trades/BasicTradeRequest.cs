using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League.Trades;

public class BasicTradeRequest
{
    [Required]
    public Guid TradeID { get; set; }
}