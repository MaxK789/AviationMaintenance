using Aviation.Maintenance.Domain.Enums;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Types;
using HotChocolate;

using MaintenanceGrpc = Aviation.Maintenance.Grpc;
using ProtoStatus = Aviation.Maintenance.Grpc.WorkOrderStatus;
using ProtoPriority = Aviation.Maintenance.Grpc.WorkOrderPriority;

namespace Aviation.WebApi.GraphQL.Resolvers;

[ExtendObjectType(typeof(AircraftGql))]
public class AircraftResolvers
{
    // aircraft { workOrders(status: IN_PROGRESS) { ... } }
    public async Task<IReadOnlyList<WorkOrderGql>> GetWorkOrdersAsync(
        [Parent] AircraftGql parent,
        WorkOrderStatus? status,
        WorkOrderPriority? priority,
        [Service] MaintenanceGrpc.WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        var req = new MaintenanceGrpc.ListWorkOrdersRequest
        {
            AircraftId = parent.Id,
            Status = status is null ? ProtoStatus.Unknown : WorkOrderGrpcMapper.ToProtoStatus(status.Value),
            Priority = priority is null ? ProtoPriority.Unknown : WorkOrderGrpcMapper.ToProtoPriority(priority.Value)
        };

        var res = await grpc.ListWorkOrdersAsync(req, cancellationToken: ct);
        return res.WorkOrders.Select(x => x.ToGql()).ToList();
    }
}
