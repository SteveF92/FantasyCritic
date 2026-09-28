namespace FantasyCritic.Web.Models.Requests.Royale;

public record SellRoyaleGameRequest(Guid PublisherID, Guid MasterGameID, decimal ExpectedRefundAmount, decimal ExpectedAdvertisingMoney);
