using Aviation.Maintenance.Domain.Entities;
using Aviation.Maintenance.Domain.Enums;

namespace Aviation.Maintenance.Domain.Interfaces;

public interface IWorkOrderService
{
    Task<WorkOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkOrder>> GetListAsync(
        int? aircraftId = null,
        WorkOrderStatus? status = null,
        WorkOrderPriority? priority = null,
        CancellationToken cancellationToken = default);

    Task<WorkOrder> CreateAsync(
        int aircraftId,
        string title,
        string? description,
        WorkOrderPriority priority,
        DateTime? plannedStart,
        DateTime? plannedEnd,
        CancellationToken cancellationToken = default);

    Task<WorkOrder> UpdateAsync(
        int id,
        string title,
        string? description,
        WorkOrderPriority priority,
        DateTime? plannedStart,
        DateTime? plannedEnd,
        CancellationToken cancellationToken = default);

    Task<WorkOrder> ChangeStatusAsync(
        int id,
        WorkOrderStatus newStatus,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
