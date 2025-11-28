using Aviation.Maintenance.Domain.Enums;

namespace Aviation.Maintenance.Domain.Entities;

public class WorkOrder
{
    public int Id { get; set; }

    public int AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public WorkOrderPriority Priority { get; set; }

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }

    public WorkOrderStatus Status { get; set; }
}
