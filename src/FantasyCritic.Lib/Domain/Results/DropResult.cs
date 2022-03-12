namespace FantasyCritic.Lib.Domain.Results;

public class DropResult
{
    public DropResult(Result result)
    {
        Result = result;
    }

    public Result Result { get; }
}