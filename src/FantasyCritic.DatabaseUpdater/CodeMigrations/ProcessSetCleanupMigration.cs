using System.Data;
using DbUp.Engine;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.MySQL;

namespace FantasyCritic.DatabaseUpdater.CodeMigrations;

/// <summary>
/// Template for a code-based migration: use this when a data fix/cleanup is too complex to express
/// as a plain SQL script but still needs to run (once) as part of a normal DbUp deployment.
///
/// Registered in Program.cs. Once it completes without throwing, it's journaled exactly like a SQL
/// script from Scripts/Sequential — it only ever runs (to completion) once per database, tracked by
/// the name it's registered under.
///
/// Note: this executes against its own repo-managed connection(s), not the same connection/
/// transaction DbUp uses for journaling (available via <paramref name="dbCommandFactory"/> in
/// <see cref="ProvideScript"/> if you need to run raw SQL directly against that connection instead).
/// </summary>
public class ProcessSetCleanupMigration : IScript
{
    private readonly RepositoryConfiguration _repositoryConfiguration;

    public ProcessSetCleanupMigration(RepositoryConfiguration repositoryConfiguration)
    {
        _repositoryConfiguration = repositoryConfiguration;
    }

    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        IFantasyCriticUserStore userStore = new MySQLFantasyCriticUserStore(_repositoryConfiguration);
        IMasterGameRepo masterGameRepo = new MySQLMasterGameRepo(_repositoryConfiguration, userStore, _repositoryConfiguration.Clock);
        ICombinedDataRepo combinedDataRepo = new MySQLCombinedDataRepo(_repositoryConfiguration, userStore);
        IFantasyCriticRepo fantasyCriticRepo = new MySQLFantasyCriticRepo(_repositoryConfiguration, userStore, masterGameRepo, combinedDataRepo);

        // TODO: implement ProcessSetCleanup here using the repos above (add more as needed,
        // following the same `new MySQLWhatever(_repositoryConfiguration, ...)` pattern).

        // Unconditional throw so this never gets journaled as applied while under development —
        // safe to re-run against a local/refreshable DB as many times as needed. Remove this once
        // the logic above is verified and you're ready to let it complete for real.
        throw new InvalidOperationException("ProcessSetCleanup is not ready to be marked as done yet.");
    }
}
