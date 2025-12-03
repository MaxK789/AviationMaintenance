using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.Maintenance.Grpc;
using Aviation.WebApi.Dtos;
using Google.Protobuf.WellKnownTypes;
using HotChocolate;
using DomainStatus = Aviation.Maintenance.Domain.Enums.WorkOrderStatus;
using DomainPriority = Aviation.Maintenance.Domain.Enums.WorkOrderPriority;
using ProtoStatus = Aviation.Maintenance.Grpc.WorkOrderStatus;
using ProtoPriority = Aviation.Maintenance.Grpc.WorkOrderPriority;
using GrpcWorkOrderService = Aviation.Maintenance.Grpc.WorkOrderService;
using GrpcWorkOrderModel = Aviation.Maintenance.Grpc.WorkOrderModel;
using GrpcListWorkOrdersRequest = Aviation.Maintenance.Grpc.ListWorkOrdersRequest;

namespace Aviation.WebApi.GraphQL;

public class RootQuery
{
    // query { aircrafts { id tailNumber model status } }
    public async Task<IEnumerable<AircraftDto>> GetAircrafts(
        AircraftStatus? status,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var entities = await aircraftService.GetListAsync(status, ct);

        return entities.Select(a => new AircraftDto
        {
            Id = a.Id,
            TailNumber = a.TailNumber,
            Model = a.Model,
            Status = a.Status
        });
    }

    // query { aircraft(id: 1) { id tailNumber model status } }
    public async Task<AircraftDto?> GetAircraft(
        int id,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var a = await aircraftService.GetByIdAsync(id, ct);
        if (a is null) return null;

        return new AircraftDto
        {
            Id = a.Id,
            TailNumber = a.TailNumber,
            Model = a.Model,
            Status = a.Status
        };
    }

    // query { workOrders(status: InProgress) { id title status aircraftTailNumber } }
    public async Task<IEnumerable<WorkOrderDto>> GetWorkOrders(
        int? aircraftId,
        DomainStatus? status,
        DomainPriority? priority,
        [Service] GrpcWorkOrderService.WorkOrderServiceClient client,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var request = new GrpcListWorkOrdersRequest
        {
            AircraftId = aircraftId ?? 0,
            Status = status.HasValue ? ToProtoStatus(status.Value) : ProtoStatus.Unknown,
            Priority = priority.HasValue ? ToProtoPriority(priority.Value) : ProtoPriority.Unknown
        };

        var response = await client.ListWorkOrdersAsync(request, cancellationToken: ct);

        var aircraftList = await aircraftService.GetListAsync(null, ct);
        var aircraftDict = aircraftList.ToDictionary(a => a.Id, a => a);

        return response.WorkOrders.Select(w =>
        {
            aircraftDict.TryGetValue(w.AircraftId, out var a);
            return MapToDto(w, a);
        });
    }

    // --------- маппинг такой же, как в контроллере ---------

    private static WorkOrderDto MapToDto(GrpcWorkOrderModel model, Aviation.Maintenance.Domain.Entities.Aircraft? aircraft)
    {
        return new WorkOrderDto
        {
            Id = model.Id,
            AircraftId = model.AircraftId,
            AircraftTailNumber = aircraft?.TailNumber ?? string.Empty,
            AircraftModel = aircraft?.Model ?? string.Empty,
            Title = model.Title,
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description,
            Priority = ToDomainPriority(model.Priority),
            PlannedStart = ToNullableDateTime(model.PlannedStart),
            PlannedEnd = ToNullableDateTime(model.PlannedEnd),
            Status = ToDomainStatus(model.Status)
        };
    }

    private static DomainStatus ToDomainStatus(ProtoStatus status) =>
        status switch
        {
            ProtoStatus.New => DomainStatus.New,
            ProtoStatus.InProgress => DomainStatus.InProgress,
            ProtoStatus.Done => DomainStatus.Done,
            ProtoStatus.Cancelled => DomainStatus.Cancelled,
            _ => DomainStatus.New
        };

    private static DomainPriority ToDomainPriority(ProtoPriority priority) =>
        priority switch
        {
            ProtoPriority.Low => DomainPriority.Low,
            ProtoPriority.Medium => DomainPriority.Medium,
            ProtoPriority.High => DomainPriority.High,
            _ => DomainPriority.Medium
        };

    private static ProtoStatus ToProtoStatus(DomainStatus status) =>
        status switch
        {
            DomainStatus.New => ProtoStatus.New,
            DomainStatus.InProgress => ProtoStatus.InProgress,
            DomainStatus.Done => ProtoStatus.Done,
            DomainStatus.Cancelled => ProtoStatus.Cancelled,
            _ => ProtoStatus.Unknown
        };

    private static ProtoPriority ToProtoPriority(DomainPriority priority) =>
        priority switch
        {
            DomainPriority.Low => ProtoPriority.Low,
            DomainPriority.Medium => ProtoPriority.Medium,
            DomainPriority.High => ProtoPriority.High,
            _ => ProtoPriority.Unknown
        };

    private static DateTime? ToNullableDateTime(Timestamp ts)
    {
        var dt = ts.ToDateTime();
        return dt == DateTime.UnixEpoch ? (DateTime?)null : dt;
    }
}
