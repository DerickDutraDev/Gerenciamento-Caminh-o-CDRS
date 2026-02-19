using TruckYard.Models;

namespace TruckYard.Dtos;

public class TruckDto
{
    public int Id { get; set; }
    public string Plate { get; set; } = "";
    public string Model { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsInYard { get; set; }

    public List<CargoDto>? Cargos { get; set; } 
    public List<MovementDto>? Movements { get; set; } 
}
