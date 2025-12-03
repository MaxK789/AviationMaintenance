using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.Dtos;

public class WorkOrderDto
{
    public int Id { get; set; }

    public int AircraftId { get; set; }
    public string AircraftTailNumber { get; set; } = null!;
    public string AircraftModel { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public WorkOrderPriority Priority { get; set; }

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }

    public WorkOrderStatus Status { get; set; }
}

public class CreateWorkOrderRequest
{
    public int AircraftId { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
}

public class UpdateWorkOrderRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public WorkOrderPriority Priority { get; set; }

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
}

public class ChangeWorkOrderStatusRequest
{
    public int Id { get; set; }

    public WorkOrderStatus NewStatus { get; set; }
}
