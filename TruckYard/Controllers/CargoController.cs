using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckYard.Data;
using TruckYard.Models;
using TruckYard.Dtos;

namespace TruckYard.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Gerencia as cargas transportadas pelos caminhões.
/// </summary>
public class CargoController : ControllerBase
{
    private readonly AppDbContext _db;

    public CargoController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lista as cargas associadas a um caminhão específico.
    /// </summary>
    /// <param name="truckId">ID do caminhão.</param>
    /// <returns>Lista de cargas do caminhão.</returns>
    [HttpGet("truck/{truckId}")]
    public async Task<IActionResult> GetCargoByTruck(int truckId)
    {
        var truck = await _db.Trucks.FindAsync(truckId);
        if (truck == null) return NotFound("Truck not found");

        var cargos = await _db.Cargos
            .Where(c => c.TruckId == truckId)
            .Select(c => new CargoDto
            {
                Id = c.Id,
                Description = c.Description,
                WeightKg = c.WeightKg,
                TruckId = c.TruckId
            })
            .ToListAsync();

        return Ok(cargos);
    }

    /// <summary>
    /// Adiciona uma nova carga a um caminhão.
    /// </summary>
    /// <param name="dto">Dados da carga.</param>
    /// <returns>Dados da carga criada.</returns>
    [HttpPost]
    public async Task<IActionResult> AddCargo([FromBody] CargoCreateDto dto)
    {
        var truck = await _db.Trucks.FindAsync(dto.TruckId);
        if (truck == null) return NotFound("Truck not found");

        var cargo = new Cargo
        {
            Description = dto.Description,
            WeightKg = dto.WeightKg,
            TruckId = dto.TruckId
        };

        _db.Cargos.Add(cargo);
        await _db.SaveChangesAsync();

        var result = new CargoDto
        {
            Id = cargo.Id,
            Description = cargo.Description,
            WeightKg = cargo.WeightKg,
            TruckId = cargo.TruckId
        };

        return Ok(result);
    }
}
