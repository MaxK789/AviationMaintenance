using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Aviation.Maintenance.Grpc.Interceptors;

public sealed class ApiKeyServerInterceptor : Interceptor
{
    private readonly IConfiguration _cfg;
    public ApiKeyServerInterceptor(IConfiguration cfg) => _cfg = cfg;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var expected = _cfg["Security:ApiKey"];
        var provided = context.RequestHeaders.GetValue("x-api-key");

        if (string.IsNullOrWhiteSpace(expected) || provided != expected)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid API key"));

        return await continuation(request, context);
    }
}
