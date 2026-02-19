namespace TruckYard.Dtos;

public class NfeDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public int TruckId { get; set; }
}
