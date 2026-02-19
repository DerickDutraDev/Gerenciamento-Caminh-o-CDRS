namespace TruckYard.Models;

public class Movement
{
    public int Id { get; set; }

    public MovementType Type { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    //truck
    public int TruckId { get; set; }
    public Truck Truck { get; set; } = null!;
}