using TruckYard.Models;
namespace TruckYard.Dtos;

public class CargoCreateDto
{
    public string Description { get; set; } = "";
    public decimal WeightKg { get; set; }
    public int TruckId { get; set; }
}
