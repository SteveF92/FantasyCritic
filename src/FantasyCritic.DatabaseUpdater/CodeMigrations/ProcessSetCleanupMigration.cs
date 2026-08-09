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
/// To wire this up, see the (commented-out) example in Program.cs. Once registered, this runs and
/// is journaled exactly like a SQL script from Scripts/Sequential — it only ever runs once per
/// database, tracked by the name it's registered under.
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

        // Code-based migrations don't have to return SQL — returning an empty string just tells
        // DbUp "this migration's work is done in C#, nothing left to execute."
        return string.Empty;
    }
}
