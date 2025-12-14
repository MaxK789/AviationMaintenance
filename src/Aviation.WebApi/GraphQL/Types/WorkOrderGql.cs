using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.GraphQL.Types;

public class WorkOrderGql
{
    public int Id { get; init; }

    public int AircraftId { get; init; }

    public string Title { get; init; } = null!;
    public string? Description { get; init; }

    public WorkOrderPriority Priority { get; init; }

    public DateTime? PlannedStart { get; init; }
    public DateTime? PlannedEnd { get; init; }

    public WorkOrderStatus Status { get; init; }
}
