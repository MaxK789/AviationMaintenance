using Aviation.Maintenance.Domain.Interfaces;
using Aviation.Maintenance.Infrastructure.Data;
using Aviation.Maintenance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aviation.Maintenance.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMaintenanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Maintenance")
                               ?? "Host=localhost;Port=5432;Database=maintenance;Username=postgres;Password=postgres";

        services.AddDbContext<MaintenanceDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IAircraftService, AircraftService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();

        return services;
    }
}
