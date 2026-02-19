using TruckYard.Models;
namespace TruckYard.Dtos;

public class MovementEntryDto
{
    public int TruckId { get; set; }
    public string? Notes { get; set; }
}
