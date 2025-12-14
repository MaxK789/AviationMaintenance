using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.GraphQL.Types;

public class AircraftGql
{
    public int Id { get; init; }
    public string TailNumber { get; init; } = null!;
    public string Model { get; init; } = null!;
    public AircraftStatus Status { get; init; }
}
