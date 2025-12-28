using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aviation.WebApi.Errors;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;
    public GlobalExceptionHandler(IHostEnvironment env) => _env = env;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var pd = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "INTERNAL_ERROR",
            Detail = _env.IsDevelopment() ? exception.ToString() : "Unexpected server error",
            Instance = context.Request.Path
        };

        pd.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(pd, ct);

        return true;
    }
}
