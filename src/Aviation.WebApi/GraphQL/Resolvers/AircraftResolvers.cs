using Aviation.Maintenance.Domain.Enums;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Types;
using HotChocolate;

using Grpc = Aviation.Maintenance.Grpc;
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
        [Service] Grpc.WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        var req = new Grpc.ListWorkOrdersRequest
        {
            AircraftId = parent.Id,
            Status = status is null ? ProtoStatus.WorkOrderStatusUnknown : WorkOrderGrpcMapper.ToProtoStatus(status.Value),
            Priority = priority is null ? ProtoPriority.WorkOrderPriorityUnknown : WorkOrderGrpcMapper.ToProtoPriority(priority.Value)
        };

        var res = await grpc.ListWorkOrdersAsync(req, cancellationToken: ct);
        return res.WorkOrders.Select(x => x.ToGql()).ToList();
    }
}
