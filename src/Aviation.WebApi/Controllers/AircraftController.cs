using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.WebApi.Dtos;
using Aviation.WebApi.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Aviation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AircraftController : ControllerBase
{
    private readonly IAircraftService _aircraftService;

    public AircraftController(IAircraftService aircraftService)
    {
        _aircraftService = aircraftService;
    }

    // GET /api/aircraft?status=InService
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AircraftDto>>> GetList(
        [FromQuery] AircraftStatus? status,
        CancellationToken ct)
    {
        var entities = await _aircraftService.GetListAsync(status, ct);

        var result = entities.Select(a => new AircraftDto
        {
            Id = a.Id,
            TailNumber = a.TailNumber,
            Model = a.Model,
            Status = a.Status
        });

        return Ok(result);
    }

    // GET /api/aircraft/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AircraftDto>> GetById(int id, CancellationToken ct)
    {
        var entity = await _aircraftService.GetByIdAsync(id, ct);
        if (entity is null) throw new NotFoundAppException($"Aircraft {id} not found");

        var dto = new AircraftDto
        {
            Id = entity.Id,
            TailNumber = entity.TailNumber,
            Model = entity.Model,
            Status = entity.Status
        };

        return Ok(dto);
    }

    // POST /api/aircraft
    [HttpPost]
    public async Task<ActionResult<AircraftDto>> Create(
        [FromBody] CreateAircraftRequest request,
        CancellationToken ct)
    {
        var entity = await _aircraftService.CreateAsync(
            request.TailNumber,
            request.Model,
            request.Status,
            ct);

        var dto = new AircraftDto
        {
            Id = entity.Id,
            TailNumber = entity.TailNumber,
            Model = entity.Model,
            Status = entity.Status
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // PUT /api/aircraft/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AircraftDto>> Update(
        int id,
        [FromBody] UpdateAircraftRequest request,
        CancellationToken ct)
    {
        var existing = await _aircraftService.GetByIdAsync(id, ct);
        if (existing is null) throw new NotFoundAppException($"Aircraft {id} not found");

        var entity = await _aircraftService.UpdateAsync(
            id,
            request.TailNumber,
            request.Model,
            request.Status,
            ct);

        var dto = new AircraftDto
        {
            Id = entity.Id,
            TailNumber = entity.TailNumber,
            Model = entity.Model,
            Status = entity.Status
        };

        return Ok(dto);
    }

    // DELETE /api/aircraft/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _aircraftService.DeleteAsync(id, ct);
        return NoContent();
    }
}
