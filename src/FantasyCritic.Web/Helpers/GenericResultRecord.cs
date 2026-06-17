using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Helpers;
public class GenericResultRecord<TValidResult> where TValidResult : class
{
    public TValidResult? ValidResult { get; }
    public ActionResult? FailedResult { get; }

    public GenericResultRecord(TValidResult? validResult, ActionResult? failedResult)
    {
        if (validResult is null && failedResult is null)
        {
            throw new Exception("Both results cannot be null");
        }

        if (validResult is not null && failedResult is not null)
        {
            throw new Exception("One result must be null.");
        }

        ValidResult = validResult;
        FailedResult = failedResult;
    }
}
