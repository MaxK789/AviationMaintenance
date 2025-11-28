using Aviation.Maintenance.Domain.Entities;
using Aviation.Maintenance.Domain.Enums;
using Aviation.Maintenance.Domain.Interfaces;
using Aviation.Maintenance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aviation.Maintenance.Infrastructure.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly MaintenanceDbContext _db;

    public WorkOrderService(MaintenanceDbContext db)
    {
        _db = db;
    }

    public Task<WorkOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.WorkOrders
            .Include(w => w.Aircraft)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetListAsync(
        int? aircraftId = null,
        WorkOrderStatus? status = null,
        WorkOrderPriority? priority = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.WorkOrders
            .Include(w => w.Aircraft)
            .AsQueryable();

        if (aircraftId.HasValue)
            query = query.Where(w => w.AircraftId == aircraftId.Value);

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(w => w.Priority == priority.Value);

        return await query
            .OrderBy(w => w.PlannedStart)
            .ThenBy(w => w.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrder> CreateAsync(
        int aircraftId,
        string title,
        string? description,
        WorkOrderPriority priority,
        DateTime? plannedStart,
        DateTime? plannedEnd,
        CancellationToken cancellationToken = default)
    {
        var entity = new WorkOrder
        {
            AircraftId = aircraftId,
            Title = title,
            Description = description,
            Priority = priority,
            PlannedStart = plannedStart,
            PlannedEnd = plannedEnd,
            Status = WorkOrderStatus.New
        };

        _db.WorkOrders.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<WorkOrder> UpdateAsync(
        int id,
        string title,
        string? description,
        WorkOrderPriority priority,
        DateTime? plannedStart,
        DateTime? plannedEnd,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
                     ?? throw new InvalidOperationException($"WorkOrder {id} not found");

        entity.Title = title;
        entity.Description = description;
        entity.Priority = priority;
        entity.PlannedStart = plannedStart;
        entity.PlannedEnd = plannedEnd;

        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<WorkOrder> ChangeStatusAsync(
        int id,
        WorkOrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
                     ?? throw new InvalidOperationException($"WorkOrder {id} not found");

        entity.Status = newStatus;

        await _db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (entity is null) return;

        _db.WorkOrders.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
