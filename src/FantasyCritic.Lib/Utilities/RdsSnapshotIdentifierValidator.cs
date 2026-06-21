using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Utilities;

public static partial class RdsSnapshotIdentifierValidator
{
    private const int MaxLength = 255;

    [GeneratedRegex("^[a-z][a-z0-9-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidIdentifierPattern();

    public static Result Validate(string snapshotIdentifier)
    {
        if (string.IsNullOrWhiteSpace(snapshotIdentifier))
        {
            return Result.Failure("Snapshot identifier is required.");
        }

        if (snapshotIdentifier.Length > MaxLength)
        {
            return Result.Failure($"Snapshot identifier must be at most {MaxLength} characters.");
        }

        if (!ValidIdentifierPattern().IsMatch(snapshotIdentifier))
        {
            return Result.Failure("Snapshot identifier must start with a letter and contain only lowercase letters, digits, and hyphens.");
        }

        return Result.Success();
    }
}
