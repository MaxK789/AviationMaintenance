using Aviation.WebApi.GraphQL.Loaders;
using Aviation.WebApi.GraphQL.Types;
using HotChocolate;

namespace Aviation.WebApi.GraphQL.Resolvers;

[ExtendObjectType(typeof(WorkOrderGql))]
public class WorkOrderResolvers
{
    // workOrder { aircraft { ... } }
    public Task<AircraftGql?> GetAircraftAsync(
        [Parent] WorkOrderGql parent,
        AircraftByIdDataLoader loader,
        CancellationToken ct)
        => loader.LoadAsync(parent.AircraftId, ct);
}
