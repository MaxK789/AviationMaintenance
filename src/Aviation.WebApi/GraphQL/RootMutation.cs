using Aviation.Maintenance.Domain.Entities;
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
using GrpcCreateWorkOrderRequest = Aviation.Maintenance.Grpc.CreateWorkOrderRequest;
using GrpcChangeWorkOrderStatusRequest = Aviation.Maintenance.Grpc.ChangeWorkOrderStatusRequest;
using CreateWorkOrderRequestDto = Aviation.WebApi.Dtos.CreateWorkOrderRequest;
using ChangeWorkOrderStatusRequestDto = Aviation.WebApi.Dtos.ChangeWorkOrderStatusRequest;

namespace Aviation.WebApi.GraphQL;

public class RootMutation
{
    // mutation {
    //   createWorkOrder(input: { aircraftId: 1, title: "...", priority: Medium }) {
    //     id title status
    //   }
    // }
    public async Task<WorkOrderDto> CreateWorkOrder(
        CreateWorkOrderRequestDto input,
        [Service] GrpcWorkOrderService.WorkOrderServiceClient client,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var grpcRequest = new GrpcCreateWorkOrderRequest
        {
            AircraftId = input.AircraftId,
            Title = input.Title,
            Description = input.Description ?? string.Empty,
            Priority = ToProtoPriority(input.Priority),
            PlannedStart = FromNullableDateTime(input.PlannedStart),
            PlannedEnd = FromNullableDateTime(input.PlannedEnd)
        };

        var response = await client.CreateWorkOrderAsync(grpcRequest, cancellationToken: ct);
        var model = response.WorkOrder;
        var aircraft = await aircraftService.GetByIdAsync(model.AircraftId, ct);

        return MapToDto(model, aircraft);
    }

    // mutation {
    //   changeWorkOrderStatus(input: { id: 10, newStatus: Done }) {
    //     id status
    //   }
    // }
    public async Task<WorkOrderDto> ChangeWorkOrderStatus(
        ChangeWorkOrderStatusRequestDto input,
        [Service] GrpcWorkOrderService.WorkOrderServiceClient client,
        [Service] IAircraftService aircraftService,
        CancellationToken ct)
    {
        var grpcRequest = new GrpcChangeWorkOrderStatusRequest
        {
            Id = input.Id,
            NewStatus = ToProtoStatus(input.NewStatus)
        };

        var response = await client.ChangeWorkOrderStatusAsync(grpcRequest, cancellationToken: ct);
        var model = response.WorkOrder;
        var aircraft = await aircraftService.GetByIdAsync(model.AircraftId, ct);

        return MapToDto(model, aircraft);
    }

    // --------- маппинг такой же, как в Query ---------

    private static WorkOrderDto MapToDto(GrpcWorkOrderModel model, Aircraft? aircraft)
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

    private static Timestamp FromNullableDateTime(DateTime? dt)
    {
        return dt.HasValue
            ? Timestamp.FromDateTime(DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc))
            : Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc));
    }
}
