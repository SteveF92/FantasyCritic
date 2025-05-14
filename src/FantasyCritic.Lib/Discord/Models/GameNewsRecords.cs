namespace FantasyCritic.Lib.Discord.Models;

public interface IGameNewsRecord
{
    MasterGame MasterGame { get; }
    LocalDate CurrentDate { get; }
}

public record NewGameNewsRecord(MasterGame MasterGame, LocalDate CurrentDate) : IGameNewsRecord;
public record EditedGameNewsRecord(MasterGame MasterGame, bool ReleaseStatusChanged, LocalDate CurrentDate) : IGameNewsRecord;
public record ReleaseGameNewsRecord(MasterGame MasterGame, LocalDate CurrentDate) : IGameNewsRecord;
public record ScoreGameNewsRecord(MasterGame MasterGame, decimal? NewScore, decimal? OldScore, LocalDate CurrentDate) : IGameNewsRecord;
