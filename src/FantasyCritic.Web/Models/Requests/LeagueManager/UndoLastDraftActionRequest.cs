using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public record UndoLastDraftActionRequest(Guid LeagueID, int Year);
