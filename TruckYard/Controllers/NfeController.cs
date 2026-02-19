using Microsoft.AspNetCore.Mvc;
using TruckYard.Data;
using TruckYard.Models;
using TruckYard.Dtos;

namespace TruckYard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NfeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public NfeController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("upload/{truckId}")]
    public async Task<IActionResult> UploadNfe(int truckId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Nenhum arquivo enviado");

        var truck = await _db.Trucks.FindAsync(truckId);
        if (truck == null)
            return NotFound("Caminhão não encontrado");

        var folder = Path.Combine(_env.ContentRootPath, "uploads");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var nfe = new Nfe
        {
            FileName = file.FileName,
            FilePath = fileName,
            TruckId = truckId
        };

        _db.Nfes.Add(nfe);
        await _db.SaveChangesAsync();

        var result = new NfeDto
        {
            Id = nfe.Id,
            FileName = nfe.FileName,
            FilePath = nfe.FilePath,
            TruckId = nfe.TruckId
        };

        return Ok(result);
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var nfe = await _db.Nfes.FindAsync(id);
        if (nfe == null) return NotFound();

        var fullPath = Path.Combine(_env.ContentRootPath, "uploads", nfe.FilePath);

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(fileBytes, "application/octet-stream", nfe.FileName);
    }
}
