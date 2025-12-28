using System.Diagnostics;
using Grpc.Core;
using HotChocolate;

namespace Aviation.WebApi.GraphQL;

public sealed class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        var traceId = Activity.Current?.Id;

        if (error.Exception is RpcException rpc)
        {
            return error
                .WithMessage("Upstream service error")
                .WithCode("GRPC_ERROR")
                .SetExtension("grpcStatus", rpc.StatusCode.ToString())
                .SetExtension("traceId", traceId);
        }

        if (error.Exception is Aviation.WebApi.Errors.AppException appEx)
        {
            return error
                .WithMessage(appEx.Message)
                .WithCode(appEx.Code)
                .SetExtension("status", appEx.StatusCode)
                .SetExtension("traceId", traceId);
        }

        return error.SetExtension("traceId", traceId);
    }
}
