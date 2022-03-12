namespace FantasyCritic.Lib.Utilities;

public class SubstringSearching
{
    public static Result<string> GetBetween(string strSource, string strStart, string strEnd)
    {
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            var start = strSource.IndexOf(strStart, 0) + strStart.Length;
            var end = strSource.IndexOf(strEnd, start);
            return Result.Success(strSource.Substring(start, end - start));
        }

        return Result.Failure<string>("Can't parse string");
    }
}