using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Requests.Admin;

public record SupportUserSearchRequest(SupportUserSearchKind SearchKind, string SearchValue);
