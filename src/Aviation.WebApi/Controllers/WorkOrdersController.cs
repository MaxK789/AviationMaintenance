using Aviation.Maintenance.Domain.Entities;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.Maintenance.Grpc;
using Aviation.WebApi.Dtos;
using Aviation.WebApi.Hubs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using DomainStatus = Aviation.Maintenance.Domain.Enums.WorkOrderStatus;
using DomainPriority = Aviation.Maintenance.Domain.Enums.WorkOrderPriority;
using ProtoStatus = Aviation.Maintenance.Grpc.WorkOrderStatus;
using ProtoPriority = Aviation.Maintenance.Grpc.WorkOrderPriority;
using GrpcWorkOrderModel = Aviation.Maintenance.Grpc.WorkOrderModel;
using GrpcCreateWorkOrderRequest = Aviation.Maintenance.Grpc.CreateWorkOrderRequest;
using GrpcUpdateWorkOrderRequest = Aviation.Maintenance.Grpc.UpdateWorkOrderRequest;
using GrpcChangeWorkOrderStatusRequest = Aviation.Maintenance.Grpc.ChangeWorkOrderStatusRequest;
using GrpcDeleteWorkOrderRequest = Aviation.Maintenance.Grpc.DeleteWorkOrderRequest;
using GrpcGetWorkOrderRequest = Aviation.Maintenance.Grpc.GetWorkOrderRequest;
using GrpcListWorkOrdersRequest = Aviation.Maintenance.Grpc.ListWorkOrdersRequest;
using CreateWorkOrderRequestDto = Aviation.WebApi.Dtos.CreateWorkOrderRequest;
using UpdateWorkOrderRequestDto = Aviation.WebApi.Dtos.UpdateWorkOrderRequest;
using ChangeWorkOrderStatusRequestDto = Aviation.WebApi.Dtos.ChangeWorkOrderStatusRequest;

namespace Aviation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderService.WorkOrderServiceClient _client;
    private readonly IAircraftService _aircraftService;
    private readonly IHubContext<MaintenanceHub> _hubContext;

    private const string DispatchersGroup = "dispatchers";

    public WorkOrdersController(
        WorkOrderService.WorkOrderServiceClient client,
        IAircraftService aircraftService,
        IHubContext<MaintenanceHub> hubContext)
    {
        _client = client;
        _aircraftService = aircraftService;
        _hubContext = hubContext;
    }

    // GET /api/workorders?aircraftId=1&status=InProgress&priority=High
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetList(
        [FromQuery] int? aircraftId,
        [FromQuery] DomainStatus? status,
        [FromQuery] DomainPriority? priority,
        CancellationToken ct)
    {
        var request = new GrpcListWorkOrdersRequest
        {
            AircraftId = aircraftId ?? 0,
            Status = status.HasValue ? ToProtoStatus(status.Value) : ProtoStatus.Unknown,
            Priority = priority.HasValue ? ToProtoPriority(priority.Value) : ProtoPriority.Unknown
        };

        var response = await _client.ListWorkOrdersAsync(request, cancellationToken: ct);

        // подгружаем данные по самолётам для красивого отображения
        var aircraftList = await _aircraftService.GetListAsync(null, ct);
        var aircraftDict = aircraftList.ToDictionary(a => a.Id, a => a);

        var result = response.WorkOrders.Select(w =>
        {
            aircraftDict.TryGetValue(w.AircraftId, out var a);
            return MapToDto(w, a);
        });

        return Ok(result);
    }

    // GET /api/workorders/10
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetWorkOrderAsync(
                new GrpcGetWorkOrderRequest { Id = id },
                cancellationToken: ct);

            var model = response.WorkOrder;
            var aircraft = await _aircraftService.GetByIdAsync(model.AircraftId, ct);

            var dto = MapToDto(model, aircraft);
            return Ok(dto);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound();
        }
    }

    // POST /api/workorders
    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(
        [FromBody] CreateWorkOrderRequestDto request,
        CancellationToken ct)
    {
        var grpcRequest = new GrpcCreateWorkOrderRequest
        {
            AircraftId = request.AircraftId,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Priority = ToProtoPriority(request.Priority),
            PlannedStart = FromNullableDateTime(request.PlannedStart),
            PlannedEnd = FromNullableDateTime(request.PlannedEnd)
        };

        var response = await _client.CreateWorkOrderAsync(grpcRequest, cancellationToken: ct);
        var model = response.WorkOrder;
        var aircraft = await _aircraftService.GetByIdAsync(model.AircraftId, ct);

        var dto = MapToDto(model, aircraft);

        await NotifyCreated(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // PUT /api/workorders/10
    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkOrderDto>> Update(
        int id,
        [FromBody] UpdateWorkOrderRequestDto request,
        CancellationToken ct)
    {
        var grpcRequest = new GrpcUpdateWorkOrderRequest
        {
            Id = id,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Priority = ToProtoPriority(request.Priority),
            PlannedStart = FromNullableDateTime(request.PlannedStart),
            PlannedEnd = FromNullableDateTime(request.PlannedEnd)
        };

        try
        {
            var response = await _client.UpdateWorkOrderAsync(grpcRequest, cancellationToken: ct);
            var model = response.WorkOrder;
            var aircraft = await _aircraftService.GetByIdAsync(model.AircraftId, ct);

            var dto = MapToDto(model, aircraft);

            await NotifyUpdated(dto, ct);
            return Ok(dto);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound();
        }
    }

    // PUT /api/workorders/10/status
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<WorkOrderDto>> ChangeStatus(
        int id,
        [FromBody] ChangeWorkOrderStatusRequestDto request,
        CancellationToken ct)
    {
        var grpcRequest = new GrpcChangeWorkOrderStatusRequest
        {
            Id = id,
            NewStatus = ToProtoStatus(request.NewStatus)
        };

        try
        {
            var response = await _client.ChangeWorkOrderStatusAsync(grpcRequest, cancellationToken: ct);
            var model = response.WorkOrder;
            var aircraft = await _aircraftService.GetByIdAsync(model.AircraftId, ct);

            var dto = MapToDto(model, aircraft);

            await NotifyUpdated(dto, ct);
            return Ok(dto);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound();
        }
    }

    // DELETE /api/workorders/10
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _client.DeleteWorkOrderAsync(new GrpcDeleteWorkOrderRequest { Id = id }, cancellationToken: ct);
        await NotifyDeleted(id, ct);
        return NoContent();
    }

    // ------------ SignalR-уведомления ------------

    private Task NotifyCreated(WorkOrderDto dto, CancellationToken ct) =>
        _hubContext.Clients.Group(DispatchersGroup)
            .SendAsync("WorkOrderCreated", dto, ct);

    private Task NotifyUpdated(WorkOrderDto dto, CancellationToken ct) =>
        _hubContext.Clients.Group(DispatchersGroup)
            .SendAsync("WorkOrderUpdated", dto, ct);

    private Task NotifyDeleted(int id, CancellationToken ct) =>
        _hubContext.Clients.Group(DispatchersGroup)
            .SendAsync("WorkOrderDeleted", id, ct);

    // ------------ Маппинги ------------

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
