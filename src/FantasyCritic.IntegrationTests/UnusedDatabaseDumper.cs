using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.IntegrationTests;

/// <summary>
/// Satisfies the ArchiveDatabase job handler that <c>AddFantasyCriticJobHandlers</c> registers. No test runs that job,
/// so this throws rather than dump anything.
/// </summary>
internal sealed class UnusedDatabaseDumper : IDatabaseDumper
{
    public string InstanceName => throw new NotSupportedException("Integration tests don't run the ArchiveDatabase job.");

    public Task<Result> DumpToGzipFile(string outputFilePath, CancellationToken cancellationToken)
        => throw new NotSupportedException("Integration tests don't run the ArchiveDatabase job.");
}
