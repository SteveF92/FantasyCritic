using NodaTime;

namespace FantasyCritic.AWS;

public sealed record ManualSnapshot(
    string Identifier,
    string SourceInstanceIdentifier,
    string Engine,
    string EngineVersion,
    Instant CreateTime,
    string MasterUsername,
    string Status);
