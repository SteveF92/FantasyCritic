using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Royale;

public class SetAdvertisingMoneyRequest
{
    [Required]
    public Guid PublisherID { get; set; }
    [Required]
    public Guid MasterGameID { get; set; }
    [Required]
    public decimal AdvertisingMoney { get; set; }
}