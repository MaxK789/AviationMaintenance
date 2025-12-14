using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.GraphQL.Inputs;

public class ChangeWorkOrderStatusInput
{
    public int Id { get; init; }
    public WorkOrderStatus NewStatus { get; init; }
}
