using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Aviation.WebApi.Grpc;

public sealed class ApiKeyClientInterceptor : Interceptor
{
    private readonly string _apiKey;
    public ApiKeyClientInterceptor(string apiKey) => _apiKey = apiKey;

    private Metadata WithKey(Metadata? headers)
    {
        headers ??= new Metadata();
        headers.Add("x-api-key", _apiKey);
        return headers;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options.WithHeaders(WithKey(context.Options.Headers));
        var ctx2 = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
        return continuation(request, ctx2);
    }
}
