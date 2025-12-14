using Aviation.Maintenance.Domain.Enums;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Types;
using HotChocolate;
using maintenance;

namespace Aviation.WebApi.GraphQL.Resolvers;

[ExtendObjectType(typeof(AircraftGql))]
public class AircraftResolvers
{
    // aircraft { workOrders(status: IN_PROGRESS) { ... } }
    public async Task<IReadOnlyList<WorkOrderGql>> GetWorkOrdersAsync(
        [Parent] AircraftGql parent,
        WorkOrderStatus? status,
        WorkOrderPriority? priority,
        [Service] WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        var req = new ListWorkOrdersRequest
        {
            AircraftId = parent.Id,
            Status = status is null ? maintenance.WorkOrderStatus.WorkOrderStatusUnknown : WorkOrderGrpcMapper.ToProtoStatus(status.Value),
            Priority = priority is null ? maintenance.WorkOrderPriority.WorkOrderPriorityUnknown : WorkOrderGrpcMapper.ToProtoPriority(priority.Value)
        };

        var res = await grpc.ListWorkOrdersAsync(req, cancellationToken: ct);
        return res.WorkOrders.Select(x => x.ToGql()).ToList();
    }
}
