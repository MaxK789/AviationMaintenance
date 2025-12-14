using Aviation.WebApi.GraphQL.Types;

namespace Aviation.WebApi.GraphQL.Payloads;

public class ChangeWorkOrderStatusPayload
{
    public WorkOrderGql? WorkOrder { get; init; }
    public IReadOnlyList<UserError>? Errors { get; init; }
}
