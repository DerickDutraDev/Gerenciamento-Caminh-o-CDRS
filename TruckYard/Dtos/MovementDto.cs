using TruckYard.Models;

namespace TruckYard.Dtos;

public class MovementDto
{
    public int Id { get; set; }
    public int TruckId { get; set; }           
    public string Type { get; set; } = "";     
    public DateTime Timestamp { get; set; }
    public string? Notes { get; set; }
}
