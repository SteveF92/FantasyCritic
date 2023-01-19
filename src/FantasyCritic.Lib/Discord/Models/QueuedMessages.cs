namespace FantasyCritic.Lib.Discord.Models;
public record NewMasterGameMessage(MasterGame MasterGame);
public record GameCriticScoreUpdateMessage(MasterGame Game, decimal? OldCriticScore, decimal? NewCriticScore);
public record MasterGameEditMessage(MasterGameYear ExistingGame, MasterGameYear EditedGame, IReadOnlyList<string> Changes);
