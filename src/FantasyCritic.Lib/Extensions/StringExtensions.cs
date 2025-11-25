using System.IO;
using System.Text.RegularExpressions;

namespace FantasyCritic.Lib.Extensions;

public static class StringExtensions
{
    public static string TrimStart(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        while (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }

    public static string CamelCaseToSpaces(this string value)
    {
        return Regex.Replace(value, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
    }

    public static string TrimStartingFromFirstInstance(this string source, string startFrom)
    {
        int index = source.IndexOf(startFrom);
        if (index > 0)
        {
            source = source.Substring(0, index);
        }

        return source;
    }

    public static string SubstringStartingFromLastInstanceOf(this string source, string startFrom)
    {
        int index = source.LastIndexOf(startFrom);
        if (index > 0)
        {
            source = source.Substring(index + startFrom.Length);
        }

        return source;
    }

    public static string EscapeForCsv(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    public static string SanitizeForFileName(this string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        return string.IsNullOrWhiteSpace(sanitized) ? "League" : sanitized;
    }
}
