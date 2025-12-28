using System.Diagnostics;
using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aviation.WebApi.Errors;

public sealed class GrpcExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;
    public GrpcExceptionHandler(IHostEnvironment env) => _env = env;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not RpcException rpc) return false;

        var (status, title) = MapGrpcToHttp(rpc.StatusCode);

        var pd = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = _env.IsDevelopment() ? rpc.Status.Detail : "Upstream service error",
            Instance = context.Request.Path
        };

        pd.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
        pd.Extensions["grpcStatus"] = rpc.StatusCode.ToString();

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(pd, ct);

        return true;
    }

    private static (int status, string title) MapGrpcToHttp(StatusCode code) => code switch
    {
        StatusCode.InvalidArgument      => (StatusCodes.Status400BadRequest, "Bad request"),
        StatusCode.FailedPrecondition   => (StatusCodes.Status400BadRequest, "Failed precondition"),
        StatusCode.Unauthenticated      => (StatusCodes.Status401Unauthorized, "Unauthenticated"),
        StatusCode.PermissionDenied     => (StatusCodes.Status403Forbidden, "Forbidden"),
        StatusCode.NotFound             => (StatusCodes.Status404NotFound, "Not found"),
        StatusCode.AlreadyExists        => (StatusCodes.Status409Conflict, "Conflict"),
        StatusCode.ResourceExhausted    => (StatusCodes.Status429TooManyRequests, "Too many requests"),
        StatusCode.DeadlineExceeded     => (StatusCodes.Status504GatewayTimeout, "Gateway timeout"),
        StatusCode.Unavailable          => (StatusCodes.Status503ServiceUnavailable, "Service unavailable"),
        _                               => (StatusCodes.Status502BadGateway, "Bad gateway")
    };
}
