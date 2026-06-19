namespace FantasyCritic.RdsSnapshotManager.Destinations;

public sealed record BackupDestinationRegistration(IBackupDestination Destination, string Prefix);
