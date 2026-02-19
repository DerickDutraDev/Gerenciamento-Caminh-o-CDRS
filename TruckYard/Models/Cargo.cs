using System.ComponentModel.DataAnnotations;

namespace TruckYard.Models;

public class Cargo
{
    public int Id { get; set; }

    [Required]
    public string Description { get; set; } = "";

    public decimal WeightKg { get; set; }

    // truck
    public int? TruckId { get; set; }

    public Truck? Truck { get; set; }
}
