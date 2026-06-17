using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace FantasyCritic.Web.OpenApi;

public class FantasyCriticOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;

        if (operation.RequestBody != null)
        {
            operation.RequestBody.IsRequired = true;
        }

        var hasAuthorize = context.MethodInfo.DeclaringType!
            .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        var hasAllowAnonymous = context.MethodInfo.DeclaringType!
            .GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
            || context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Responses["401"] = new OpenApiResponse { Description = "Unauthorized" };
        }

        var httpMethod = context.OperationDescription.Method.ToUpperInvariant();
        if (httpMethod is "POST" or "PUT" or "PATCH")
        {
            operation.Responses["400"] = new OpenApiResponse { Description = "Bad Request" };
        }

        return true;
    }
}
