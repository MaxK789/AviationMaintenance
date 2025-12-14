using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.WebApi.GraphQL.Inputs;
using Aviation.WebApi.GraphQL.Mappers;
using Aviation.WebApi.GraphQL.Types;
using HotChocolate;
using maintenance;

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

    // Пагинация "по-графкьюэльному" (offset paging) + filter input
    public async Task<WorkOrdersPage> GetWorkOrdersPage(
        WorkOrderFilterInput? filter,
        int skip = 0,
        int take = 20,
        [Service] WorkOrderService.WorkOrderServiceClient grpc,
        CancellationToken ct = default)
    {
        var request = new ListWorkOrdersRequest
        {
            AircraftId = filter?.AircraftId ?? 0,
            Status = filter?.Status is null ? WorkOrderStatus.WorkOrderStatusUnknown : WorkOrderGrpcMapper.ToProtoStatus(filter.Status.Value),
            Priority = filter?.Priority is null ? WorkOrderPriority.WorkOrderPriorityUnknown : WorkOrderGrpcMapper.ToProtoPriority(filter.Priority.Value)
        };

        var response = await grpc.ListWorkOrdersAsync(request, cancellationToken: ct);

        var all = response.WorkOrders.Select(x => x.ToGql()).ToList();
        var page = all.Skip(Math.Max(skip, 0)).Take(Math.Max(take, 0)).ToList();

        return new WorkOrdersPage
        {
            Items = page,
            TotalCount = all.Count
        };
    }
}
