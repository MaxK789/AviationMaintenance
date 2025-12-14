using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.GraphQL.Inputs;

public class WorkOrderFilterInput
{
    public int? AircraftId { get; init; }
    public WorkOrderStatus? Status { get; init; }
    public WorkOrderPriority? Priority { get; init; }
}
