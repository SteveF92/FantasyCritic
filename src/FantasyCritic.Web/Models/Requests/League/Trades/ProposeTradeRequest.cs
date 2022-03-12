using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.League.Trades;

public class ProposeTradeRequest
{
    [Required]
    public Guid ProposerPublisherID { get; set; }
    [Required]
    public Guid CounterPartyPublisherID { get; set; }
    [Required]
    public List<Guid> ProposerPublisherGameIDs { get; set; }
    [Required]
    public List<Guid> CounterPartyPublisherGameIDs { get; set; }
    [Required]
    public uint ProposerBudgetSendAmount { get; set; }
    [Required]
    public uint CounterPartyBudgetSendAmount { get; set; }
    [Required]
    public string Message { get; set; }
}