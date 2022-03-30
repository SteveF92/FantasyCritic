namespace FantasyCritic.Web.Models.Requests.Royale;

public record SetAdvertisingMoneyRequest(Guid PublisherID, Guid MasterGameID, decimal AdvertisingMoney);
