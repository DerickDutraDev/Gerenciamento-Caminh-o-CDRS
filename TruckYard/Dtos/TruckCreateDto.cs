using TruckYard.Models;

namespace TruckYard.Dtos;

public class TruckCreateDto
{
    public string Plate { get; set; } = "";
    public string Model { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsInYard { get; set; }
}
