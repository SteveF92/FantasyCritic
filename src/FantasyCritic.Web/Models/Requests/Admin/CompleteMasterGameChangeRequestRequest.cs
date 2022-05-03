using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin;

public record CompleteMasterGameChangeRequestRequest([Required] Guid RequestID, [Required] string ResponseNote);
