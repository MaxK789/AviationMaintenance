using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.WebApi.GraphQL.Inputs;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Types;
using Grpc.Core;

using MaintenanceGrpc = Aviation.Maintenance.Grpc;
using ProtoStatus = Aviation.Maintenance.Grpc.WorkOrderStatus;
using ProtoPriority = Aviation.Maintenance.Grpc.WorkOrderPriority;

namespace Aviation.WebApi.GraphQL;

public class Query
{
    public async Task<IReadOnlyList<AircraftGql>> GetAircrafts(
        AircraftStatus? status,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var list = await aircraftService.GetListAsync(status, ct);

        return list.Select(a => new AircraftGql
        {
            Id = a.Id,
            TailNumber = a.TailNumber,
            Model = a.Model,
            Status = a.Status
        }).ToList();
    }

    public async Task<AircraftGql?> GetAircraft(
        int id,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var a = await aircraftService.GetByIdAsync(id, ct);
        if (a is null) return null;

        return new AircraftGql
        {
            Id = a.Id,
            TailNumber = a.TailNumber,
            Model = a.Model,
            Status = a.Status
        };
    }

    public async Task<WorkOrdersPage> GetWorkOrdersPage(
        WorkOrderFilterInput? filter,
        [Service] MaintenanceGrpc.WorkOrderService.WorkOrderServiceClient grpc,
        int skip = 0,
        int take = 20,
        CancellationToken ct = default)
    {
        var request = new MaintenanceGrpc.ListWorkOrdersRequest
        {
            AircraftId = filter?.AircraftId ?? 0,
            Status = filter?.Status is null ? ProtoStatus.Unknown : WorkOrderGrpcMapper.ToProtoStatus(filter.Status.Value),
            Priority = filter?.Priority is null ? ProtoPriority.Unknown : WorkOrderGrpcMapper.ToProtoPriority(filter.Priority.Value)
        };

        var response = await grpc.ListWorkOrdersAsync(request, cancellationToken: ct);

        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 100);

        var all = response.WorkOrders.Select(x => x.ToGql()).ToList();
        var page = all.Skip(skip).Take(take).ToList();

        return new WorkOrdersPage
        {
            Items = page,
            TotalCount = all.Count
        };
    }

    public async Task<WorkOrderGql?> GetWorkOrder(
        int id,
        [Service] MaintenanceGrpc.WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct)
    {
        try
        {
            var res = await grpc.GetWorkOrderAsync(
                new MaintenanceGrpc.GetWorkOrderRequest { Id = id },
                cancellationToken: ct);

            return res.WorkOrder.ToGql();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
