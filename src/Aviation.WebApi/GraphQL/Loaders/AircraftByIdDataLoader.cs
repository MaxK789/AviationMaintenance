using Aviation.Maintenance.Infrastructure.Data;
using Aviation.WebApi.GraphQL.Types;
using GreenDonut;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aviation.WebApi.GraphQL.Loaders;

public class AircraftByIdDataLoader : BatchDataLoader<int, AircraftGql>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AircraftByIdDataLoader(
        IServiceScopeFactory scopeFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<IReadOnlyDictionary<int, AircraftGql>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();

        var list = await db.Aircraft
            .Where(a => keys.Contains(a.Id))
            .Select(a => new AircraftGql
            {
                Id = a.Id,
                TailNumber = a.TailNumber,
                Model = a.Model,
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        return list.ToDictionary(x => x.Id, x => x);
    }
}
