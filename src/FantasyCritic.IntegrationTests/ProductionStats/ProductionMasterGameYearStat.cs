using System;

namespace FantasyCritic.IntegrationTests.ProductionStats;

/// <summary>
/// Minimal production master-game-year stats used by integration test selection strategies.
/// </summary>
internal sealed record ProductionMasterGameYearStat(
    Guid MasterGameID,
    string GameName,
    decimal HypeFactor);
