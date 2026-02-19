using System.ComponentModel.DataAnnotations;

namespace TruckYard.Models;

public class Truck
{
    public int Id { get; set; }

    [Required]
    public string Plate { get; set; } = null!;

    public string Model { get; set; } = "";

    public string DriverName { get; set; } = "";

    public TruckStatus Status { get; set; } = TruckStatus.Available;

    public bool IsInYard { get; set; } = false;

    public DateTime? LastUpdate { get; set; }

    public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
    public ICollection<Nfe> NFes { get; set; } = new List<Nfe>();
}
