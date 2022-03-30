using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin;

public record CompleteMasterGameRequestRequest([Required] Guid RequestID, [Required] string ResponseNote, Guid? MasterGameID);
