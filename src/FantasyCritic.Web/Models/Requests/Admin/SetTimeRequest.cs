using NodaTime;

namespace FantasyCritic.Web.Models.Requests.Admin;

public record SetTimeRequest(Instant NewTime);
