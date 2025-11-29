using Aviation.Maintenance.Domain.Enums;

namespace Aviation.WebApi.Dtos;

public class AircraftDto
{
    public int Id { get; set; }
    public string TailNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public AircraftStatus Status { get; set; }
}

public class CreateAircraftRequest
{
    public string TailNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public AircraftStatus Status { get; set; } = AircraftStatus.InService;
}

public class UpdateAircraftRequest
{
    public string TailNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public AircraftStatus Status { get; set; }
}
