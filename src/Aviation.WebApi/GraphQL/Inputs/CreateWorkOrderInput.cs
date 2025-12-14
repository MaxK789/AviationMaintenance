using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.GraphQL.Inputs;

public class CreateWorkOrderInput
{
    public int AircraftId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public WorkOrderPriority Priority { get; init; } = WorkOrderPriority.Medium;
    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedEnd { get; init; }
}
