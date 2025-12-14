namespace Aviation.WebApi.GraphQL.Types;

public class WorkOrdersPage
{
    public IReadOnlyList<WorkOrderGql> Items { get; init; } = Array.Empty<WorkOrderGql>();
    public int TotalCount { get; init; }
}
