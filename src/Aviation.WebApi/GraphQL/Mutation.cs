using Aviation.WebApi.GraphQL.Inputs;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Payloads;
using HotChocolate;
using maintenance;
using Grpc.Core;

namespace Aviation.WebApi.GraphQL;

public class Mutation
{
    public async Task<CreateWorkOrderPayload> CreateWorkOrder(
        CreateWorkOrderInput input,
        [Service] WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        try
        {
            var req = new CreateWorkOrderRequest
            {
                AircraftId = input.AircraftId,
                Title = input.Title,
                Description = input.Description ?? string.Empty,
                Priority = WorkOrderGrpcMapper.ToProtoPriority(input.Priority),
                PlannedStart = WorkOrderGrpcMapper.FromNullableDateTime(input.PlannedStart),
                PlannedEnd = WorkOrderGrpcMapper.FromNullableDateTime(input.PlannedEnd)
            };

            var res = await grpc.CreateWorkOrderAsync(req, cancellationToken: ct);

            return new CreateWorkOrderPayload
            {
                WorkOrder = res.WorkOrder.ToGql()
            };
        }
        catch (RpcException ex)
        {
            return new CreateWorkOrderPayload
            {
                Errors = new[]
                {
                    new UserError { Code = ex.StatusCode.ToString(), Message = ex.Status.Detail }
                }
            };
        }
    }

    public async Task<ChangeWorkOrderStatusPayload> ChangeWorkOrderStatus(
        ChangeWorkOrderStatusInput input,
        [Service] WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        try
        {
            var req = new ChangeWorkOrderStatusRequest
            {
                Id = input.Id,
                NewStatus = WorkOrderGrpcMapper.ToProtoStatus(input.NewStatus)
            };

            var res = await grpc.ChangeWorkOrderStatusAsync(req, cancellationToken: ct);

            return new ChangeWorkOrderStatusPayload
            {
                WorkOrder = res.WorkOrder.ToGql()
            };
        }
        catch (RpcException ex)
        {
            return new ChangeWorkOrderStatusPayload
            {
                Errors = new[]
                {
                    new UserError { Code = ex.StatusCode.ToString(), Message = ex.Status.Detail }
                }
            };
        }
    }
}
