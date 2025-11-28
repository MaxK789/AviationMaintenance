using Aviation.Maintenance.Domain.Enums;

namespace Aviation.Maintenance.Domain.Entities;

public class Aircraft
{
    public int Id { get; set; }
    public string TailNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public AircraftStatus Status { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
