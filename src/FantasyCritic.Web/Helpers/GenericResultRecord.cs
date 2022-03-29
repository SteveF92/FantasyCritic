using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCritic.Web.Helpers;
public class GenericResultRecord<TValidResult> where TValidResult : class
{
    [MemberNotNullWhen(returnValue: true, member: nameof(FailedResult))]
    public bool HasValidResult => ValidResult is not null;

    [MemberNotNullWhen(returnValue: true, member: nameof(ValidResult))]
    public bool HasFailedResult => FailedResult is not null;

    public TValidResult? ValidResult { get; }
    public IActionResult? FailedResult { get; }

    public GenericResultRecord(TValidResult? validResult, IActionResult? failedResult)
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
