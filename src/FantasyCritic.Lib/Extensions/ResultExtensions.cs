namespace FantasyCritic.Lib.Extensions;
public static class ResultExtensions
{
    public static T? ToNullable<T>(this Result<T> result) where T : class
    {
        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }
}
