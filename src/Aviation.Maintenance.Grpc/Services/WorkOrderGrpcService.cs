using Aviation.Maintenance.Domain.Interfaces;
using Aviation.Maintenance.Grpc.Mapping;
using Grpc.Core;
using Aviation.Maintenance.Grpc;
using DomainPriority = Aviation.Maintenance.Domain.Enums.WorkOrderPriority;
using DomainStatus = Aviation.Maintenance.Domain.Enums.WorkOrderStatus;
using ProtoPriority = Aviation.Maintenance.Grpc.WorkOrderPriority;
using ProtoStatus = Aviation.Maintenance.Grpc.WorkOrderStatus;

namespace Aviation.Maintenance.Grpc.Services;

public sealed class WorkOrderGrpcService : WorkOrderService.WorkOrderServiceBase
{
    private readonly IWorkOrderService _workOrders;

    public WorkOrderGrpcService(IWorkOrderService workOrders)
    {
        _workOrders = workOrders;
    }

    public override async Task<GetWorkOrderResponse> GetWorkOrder(
        GetWorkOrderRequest request,
        ServerCallContext context)
    {
        var entity = await _workOrders.GetByIdAsync(request.Id, context.CancellationToken);
        if (entity is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"WorkOrder {request.Id} not found"));
        }

        return new GetWorkOrderResponse
        {
            WorkOrder = MapToModel(entity)
        };
    }

    public override async Task<ListWorkOrdersResponse> ListWorkOrders(
        ListWorkOrdersRequest request,
        ServerCallContext context)
    {
        DomainStatus? status = request.Status == ProtoStatus.Unknown
            ? null
            : request.Status.ToDomainStatus();

        DomainPriority? priority = request.Priority == ProtoPriority.Unknown
            ? null
            : request.Priority.ToDomainPriority();

        var list = await _workOrders.GetListAsync(
            request.AircraftId == 0 ? null : request.AircraftId,
            status,
            priority,
            context.CancellationToken);

        var response = new ListWorkOrdersResponse();
        response.WorkOrders.AddRange(list.Select(MapToModel));
        return response;
    }

    public override async Task<CreateWorkOrderResponse> CreateWorkOrder(
        CreateWorkOrderRequest request,
        ServerCallContext context)
    {
        var entity = await _workOrders.CreateAsync(
            request.AircraftId,
            request.Title,
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
            request.Priority.ToDomainPriority(),
            request.PlannedStart.ToNullableDateTime(),
            request.PlannedEnd.ToNullableDateTime(),
            context.CancellationToken);

        return new CreateWorkOrderResponse
        {
            WorkOrder = MapToModel(entity)
        };
    }

    public override async Task<UpdateWorkOrderResponse> UpdateWorkOrder(
        UpdateWorkOrderRequest request,
        ServerCallContext context)
    {
        try
        {
            var entity = await _workOrders.UpdateAsync(
                request.Id,
                request.Title,
                string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
                request.Priority.ToDomainPriority(),
                request.PlannedStart.ToNullableDateTime(),
                request.PlannedEnd.ToNullableDateTime(),
                context.CancellationToken);

            return new UpdateWorkOrderResponse
            {
                WorkOrder = MapToModel(entity)
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<ChangeWorkOrderStatusResponse> ChangeWorkOrderStatus(
        ChangeWorkOrderStatusRequest request,
        ServerCallContext context)
    {
        try
        {
            var entity = await _workOrders.ChangeStatusAsync(
                request.Id,
                request.NewStatus.ToDomainStatus(),
                context.CancellationToken);

            return new ChangeWorkOrderStatusResponse
            {
                WorkOrder = MapToModel(entity)
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<DeleteWorkOrderResponse> DeleteWorkOrder(
        DeleteWorkOrderRequest request,
        ServerCallContext context)
    {
        await _workOrders.DeleteAsync(request.Id, context.CancellationToken);
        return new DeleteWorkOrderResponse();
    }

    private static WorkOrderModel MapToModel(Aviation.Maintenance.Domain.Entities.WorkOrder entity)
    {
        return new WorkOrderModel
        {
            Id = entity.Id,
            AircraftId = entity.AircraftId,
            Title = entity.Title,
            Description = entity.Description ?? string.Empty,
            Priority = entity.Priority.ToProtoPriority(),
            PlannedStart = entity.PlannedStart.ToTimestampOrDefault(),
            PlannedEnd = entity.PlannedEnd.ToTimestampOrDefault(),
            Status = entity.Status.ToProtoStatus()
        };
    }
}
