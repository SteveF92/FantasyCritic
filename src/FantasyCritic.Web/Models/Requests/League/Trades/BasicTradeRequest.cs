using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League.Trades;

public class BasicTradeRequest
{
    [Required]
    public Guid LeagueID { get; set; }
    [Required]
    public int Year { get; set; }
    [Required]
    public Guid TradeID { get; set; }
}
