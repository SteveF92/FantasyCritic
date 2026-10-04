using FantasyCritic.AWS;

namespace FantasyCritic.RdsSnapshotManager.Services;

public enum SnapshotArchiveOutcome
{
    Archived,
    AlreadyArchived,
    Skipped,
    Failed,
}

public sealed record ArchivedFile(string Key, long Bytes);

public sealed record SnapshotArchiveResult(
    string SnapshotIdentifier,
    SnapshotArchiveOutcome Outcome,
    string Detail,
    IReadOnlyList<ArchivedFile> Files);

public sealed record SnapshotArchiveManifest(
    string SnapshotIdentifier,
    string SourceInstanceIdentifier,
    string Engine,
    string EngineVersion,
    string SnapshotCreateTime,
    string ArchivedAt,
    IReadOnlyList<ArchivedFile> Files);

public sealed record ArchivedSnapshotDump(string SnapshotIdentifier, ArchivedObject Object);
