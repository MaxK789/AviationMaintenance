using Aviation.Maintenance.Domain.Entities;
using Aviation.Maintenance.Domain.Enums;

namespace Aviation.Maintenance.Domain.Interfaces;

public interface IAircraftService
{
    Task<IReadOnlyList<Aircraft>> GetListAsync(
        AircraftStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Aircraft?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Aircraft> CreateAsync(
        string tailNumber,
        string model,
        AircraftStatus status,
        CancellationToken cancellationToken = default);

    Task<Aircraft> UpdateAsync(
        int id,
        string tailNumber,
        string model,
        AircraftStatus status,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
