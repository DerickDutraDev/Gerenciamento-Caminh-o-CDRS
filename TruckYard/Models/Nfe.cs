namespace TruckYard.Models;

public class Nfe
{
    public int Id { get; set; }

    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    
    public int TruckId { get; set; }
    public Truck Truck { get; set; } = null!;
}
