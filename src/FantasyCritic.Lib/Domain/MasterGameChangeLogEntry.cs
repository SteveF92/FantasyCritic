using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;
public record MasterGameChangeLogEntry(Guid MasterGameChangeID, MasterGame MasterGame, FantasyCriticUser ChangedByUser, Instant Timestamp, string Change);
