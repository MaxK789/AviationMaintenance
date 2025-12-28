using Aviation.WebApi.GraphQL.Inputs;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Payloads;

using MaintenanceGrpc = Aviation.Maintenance.Grpc;

namespace Aviation.WebApi.GraphQL;

public class Mutation
{
    public async Task<CreateWorkOrderPayload> CreateWorkOrder(
        CreateWorkOrderInput input,
        [Service] MaintenanceGrpc.WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        var req = new MaintenanceGrpc.CreateWorkOrderRequest
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

    public async Task<ChangeWorkOrderStatusPayload> ChangeWorkOrderStatus(
        ChangeWorkOrderStatusInput input,
        [Service] MaintenanceGrpc.WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        var req = new MaintenanceGrpc.ChangeWorkOrderStatusRequest
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
}
