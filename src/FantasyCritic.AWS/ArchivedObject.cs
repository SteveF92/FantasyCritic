namespace FantasyCritic.AWS;

public sealed record ArchivedObject(string Key, long Bytes, ArchivedObjectAvailability Availability);

public enum ArchivedObjectAvailability
{
    Available,
    InGlacier,
    GlacierRestoreInProgress,
}
