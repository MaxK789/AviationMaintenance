using Aviation.Maintenance.Domain.Entities;
using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.Maintenance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aviation.Maintenance.Infrastructure.Services;

public class AircraftService : IAircraftService
{
    private readonly MaintenanceDbContext _db;

    public AircraftService(MaintenanceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Aircraft>> GetListAsync(
        AircraftStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Aircraft.AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.TailNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<Aircraft?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Aircraft.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Aircraft> CreateAsync(
        string tailNumber,
        string model,
        AircraftStatus status,
        CancellationToken cancellationToken = default)
    {
        var entity = new Aircraft
        {
            TailNumber = tailNumber,
            Model = model,
            Status = status
        };

        _db.Aircraft.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Aircraft> UpdateAsync(
        int id,
        string tailNumber,
        string model,
        AircraftStatus status,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Aircraft.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
                     ?? throw new InvalidOperationException($"Aircraft {id} not found");

        entity.TailNumber = tailNumber;
        entity.Model = model;
        entity.Status = status;

        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Aircraft.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity is null) return;

        _db.Aircraft.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
