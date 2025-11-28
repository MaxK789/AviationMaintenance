using Aviation.Maintenance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aviation.Maintenance.Infrastructure.Data;

public class MaintenanceDbContext : DbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Aircraft> Aircraft { get; set; } = null!;
    public DbSet<WorkOrder> WorkOrders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaintenanceDbContext).Assembly);
    }
}
