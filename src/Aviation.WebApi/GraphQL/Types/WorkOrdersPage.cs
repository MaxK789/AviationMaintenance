namespace Aviation.WebApi.GraphQL.Types;

public class WorkOrdersPage
{
    public required IReadOnlyList<WorkOrderGql> Items { get; init; }
    public required int TotalCount { get; init; }
}
