using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckYard.Data;
using TruckYard.Models;
using TruckYard.Dtos;

namespace TruckYard.Controllers;

/// <summary>
/// Gerencia as operações relacionadas aos caminhões.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TrucksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TrucksController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Obtém todos os caminhões cadastrados.
    /// </summary>
    /// <returns>Lista de caminhões e seus status.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var trucks = await _db.Trucks
            .Include(t => t.Cargos)
            .Include(t => t.Movements)
            .ToListAsync();

        var dto = trucks.Select(t => new TruckDto
        {
            Id = t.Id,
            Plate = t.Plate,
            Model = t.Model,
            DriverName = t.DriverName,
            Status = t.Status.ToString(),
            IsInYard = t.IsInYard,
            Cargos = t.Cargos.Select(c => new CargoDto
            {
                Id = c.Id,
                Description = c.Description,
                WeightKg = c.WeightKg
            }).ToList(),
            Movements = t.Movements.Select(m => new MovementDto
            {
                Id = m.Id,
                Type = m.Type.ToString(),
                Notes = m.Notes
            }).ToList()
        });

        return Ok(dto);
    }

    /// <summary>
    /// Obtém apenas os caminhões que estão fisicamente no pátio.
    /// </summary>
    /// <returns>Lista de caminhões no pátio.</returns>
    [HttpGet("patio")]
    public async Task<IActionResult> GetYard()
    {
        var trucks = await _db.Trucks
            .Where(t => t.IsInYard)
            .Include(t => t.Cargos)
            .Include(t => t.Movements)
            .ToListAsync();

        var dto = trucks.Select(t => new TruckDto
        {
            Id = t.Id,
            Plate = t.Plate,
            Model = t.Model,
            DriverName = t.DriverName,
            Status = t.Status.ToString(),
            IsInYard = t.IsInYard,
            Cargos = t.Cargos.Select(c => new CargoDto
            {
                Id = c.Id,
                Description = c.Description,
                WeightKg = c.WeightKg
            }).ToList(),
            Movements = t.Movements.Select(m => new MovementDto
            {
                Id = m.Id,
                Type = m.Type.ToString(),
                Notes = m.Notes
            }).ToList()
        });

        return Ok(dto);
    }

    /// <summary>
    /// Busca um caminhão específico pelo ID.
    /// </summary>
    /// <param name="id">ID do caminhão.</param>
    /// <returns>Dados detalhados do caminhão.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _db.Trucks
            .Include(t => t.Cargos)
            .Include(t => t.Movements)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (t == null) return NotFound("Truck not found");

        var dto = new TruckDto
        {
            Id = t.Id,
            Plate = t.Plate,
            Model = t.Model,
            DriverName = t.DriverName,
            Status = t.Status.ToString(),
            IsInYard = t.IsInYard,
            Cargos = t.Cargos.Select(c => new CargoDto
            {
                Id = c.Id,
                Description = c.Description,
                WeightKg = c.WeightKg
            }).ToList(),
            Movements = t.Movements.Select(m => new MovementDto
            {
                Id = m.Id,
                Type = m.Type.ToString(),
                Notes = m.Notes
            }).ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Cadastra um novo caminhão.
    /// </summary>
    /// <param name="dto">Dados para criação do caminhão.</param>
    /// <returns>ID do caminhão criado.</returns>
    [HttpPost]
    public async Task<IActionResult> AddTruck([FromBody] TruckCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Enum.TryParse<TruckStatus>(dto.Status, true, out var status))
        {
            return BadRequest($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TruckStatus>())}");
        }

        var truck = new Truck
        {
            Plate = dto.Plate,
            Model = dto.Model,
            DriverName = dto.DriverName,
            Status = status,
            IsInYard = dto.IsInYard
        };  

        _db.Trucks.Add(truck);
        await _db.SaveChangesAsync();

        return Ok(new { truck.Id });
    }

    /// <summary>
    /// Atualiza os dados de um caminhão existente.
    /// </summary>
    /// <param name="id">ID do caminhão.</param>
    /// <param name="dto">Dados a serem atualizados.</param>
    /// <returns>Status da operação.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TruckUpdateDto dto)
    {
        var t = await _db.Trucks.FindAsync(id);
        if (t == null) return NotFound("Truck not found");

        if (dto.Plate != null) t.Plate = dto.Plate;
        if (dto.Model != null) t.Model = dto.Model;
        if (dto.DriverName != null) t.DriverName = dto.DriverName;

        if (dto.Status != null)
        {
            if (!Enum.TryParse<TruckStatus>(dto.Status, true, out var status))
            {
                return BadRequest($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<TruckStatus>())}");
            }
            t.Status = status;
        }

        if (dto.IsInYard.HasValue) 
            t.IsInYard = dto.IsInYard.Value;

        t.LastUpdate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { updated = true });
    }

    /// <summary>
    /// Remove um caminhão do sistema.
    /// </summary>
    /// <param name="id">ID do caminhão a ser removido.</param>
    /// <returns>Mensagem de confirmação.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.Trucks.FindAsync(id);
        if (t == null) return NotFound();

        _db.Trucks.Remove(t);
        await _db.SaveChangesAsync();
        return Ok("Truck removed");
    }
}
