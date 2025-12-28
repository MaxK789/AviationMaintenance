using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aviation.WebApi.Errors;

public sealed class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not AppException appEx) return false;

        var pd = new ProblemDetails
        {
            Status = appEx.StatusCode,
            Title = appEx.Code,
            Detail = appEx.Message,
            Instance = context.Request.Path
        };

        pd.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.StatusCode = appEx.StatusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(pd, ct);

        return true;
    }
}
