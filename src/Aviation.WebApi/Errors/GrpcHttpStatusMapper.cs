using Grpc.Core;

namespace Aviation.WebApi.Errors;

public static class GrpcHttpStatusMapper
{
    public static (int status, string title) Map(StatusCode code) => code switch
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
