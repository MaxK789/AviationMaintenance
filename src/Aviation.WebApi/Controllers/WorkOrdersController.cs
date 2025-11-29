using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Aviation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    // GET /api/workorders?aircraftId=1&status=InProgress&priority=High
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetList(
        [FromQuery] int? aircraftId,
        [FromQuery] WorkOrderStatus? status,
        [FromQuery] WorkOrderPriority? priority,
        CancellationToken ct)
    {
        var entities = await _workOrderService.GetListAsync(
            aircraftId,
            status,
            priority,
            ct);

        var result = entities.Select(w => new WorkOrderDto
        {
            Id = w.Id,
            AircraftId = w.AircraftId,
            AircraftTailNumber = w.Aircraft.TailNumber,
            AircraftModel = w.Aircraft.Model,
            Title = w.Title,
            Description = w.Description,
            Priority = w.Priority,
            PlannedStart = w.PlannedStart,
            PlannedEnd = w.PlannedEnd,
            Status = w.Status
        });

        return Ok(result);
    }

    // GET /api/workorders/10
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(int id, CancellationToken ct)
    {
        var w = await _workOrderService.GetByIdAsync(id, ct);
        if (w is null) return NotFound();

        var dto = new WorkOrderDto
        {
            Id = w.Id,
            AircraftId = w.AircraftId,
            AircraftTailNumber = w.Aircraft.TailNumber,
            AircraftModel = w.Aircraft.Model,
            Title = w.Title,
            Description = w.Description,
            Priority = w.Priority,
            PlannedStart = w.PlannedStart,
            PlannedEnd = w.PlannedEnd,
            Status = w.Status
        };

        return Ok(dto);
    }

    // POST /api/workorders
    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(
        [FromBody] CreateWorkOrderRequest request,
        CancellationToken ct)
    {
        var entity = await _workOrderService.CreateAsync(
            request.AircraftId,
            request.Title,
            request.Description,
            request.Priority,
            request.PlannedStart,
            request.PlannedEnd,
            ct);

        // Чтобы был Aircraft, можно повторно получить сущность или настроить Include в сервисе
        var w = await _workOrderService.GetByIdAsync(entity.Id, ct) ?? entity;

        var dto = new WorkOrderDto
        {
            Id = w.Id,
            AircraftId = w.AircraftId,
            AircraftTailNumber = w.Aircraft?.TailNumber ?? string.Empty,
            AircraftModel = w.Aircraft?.Model ?? string.Empty,
            Title = w.Title,
            Description = w.Description,
            Priority = w.Priority,
            PlannedStart = w.PlannedStart,
            PlannedEnd = w.PlannedEnd,
            Status = w.Status
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // PUT /api/workorders/10
    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkOrderDto>> Update(
        int id,
        [FromBody] UpdateWorkOrderRequest request,
        CancellationToken ct)
    {
        try
        {
            var entity = await _workOrderService.UpdateAsync(
                id,
                request.Title,
                request.Description,
                request.Priority,
                request.PlannedStart,
                request.PlannedEnd,
                ct);

            var w = await _workOrderService.GetByIdAsync(entity.Id, ct) ?? entity;

            var dto = new WorkOrderDto
            {
                Id = w.Id,
                AircraftId = w.AircraftId,
                AircraftTailNumber = w.Aircraft?.TailNumber ?? string.Empty,
                AircraftModel = w.Aircraft?.Model ?? string.Empty,
                Title = w.Title,
                Description = w.Description,
                Priority = w.Priority,
                PlannedStart = w.PlannedStart,
                PlannedEnd = w.PlannedEnd,
                Status = w.Status
            };

            return Ok(dto);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    // PUT /api/workorders/10/status
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<WorkOrderDto>> ChangeStatus(
        int id,
        [FromBody] ChangeWorkOrderStatusRequest request,
        CancellationToken ct)
    {
        try
        {
            var entity = await _workOrderService.ChangeStatusAsync(
                id,
                request.NewStatus,
                ct);

            var w = await _workOrderService.GetByIdAsync(entity.Id, ct) ?? entity;

            var dto = new WorkOrderDto
            {
                Id = w.Id,
                AircraftId = w.AircraftId,
                AircraftTailNumber = w.Aircraft?.TailNumber ?? string.Empty,
                AircraftModel = w.Aircraft?.Model ?? string.Empty,
                Title = w.Title,
                Description = w.Description,
                Priority = w.Priority,
                PlannedStart = w.PlannedStart,
                PlannedEnd = w.PlannedEnd,
                Status = w.Status
            };

            return Ok(dto);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    // DELETE /api/workorders/10
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _workOrderService.DeleteAsync(id, ct);
        return NoContent();
    }
}
