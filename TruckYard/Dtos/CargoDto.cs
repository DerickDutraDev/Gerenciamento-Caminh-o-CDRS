namespace TruckYard.Dtos;

public class CargoDto
{
	public int Id { get; set; }
	public string Description { get; set; } = string.Empty;
	public decimal WeightKg { get; set; }
	public int? TruckId { get; set; }
}
