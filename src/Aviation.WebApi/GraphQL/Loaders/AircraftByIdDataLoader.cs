using Aviation.Maintenance.Infrastructure.Data;
using Aviation.WebApi.GraphQL.Types;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace Aviation.WebApi.GraphQL.Loaders;

public class AircraftByIdDataLoader : BatchDataLoader<int, AircraftGql>
{
    private readonly IDbContextFactory<MaintenanceDbContext> _dbFactory;

    public AircraftByIdDataLoader(
        IDbContextFactory<MaintenanceDbContext> dbFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dbFactory = dbFactory;
    }

    protected override async Task<IReadOnlyDictionary<int, AircraftGql>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

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
