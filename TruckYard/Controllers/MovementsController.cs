using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckYard.Data;
using TruckYard.Models;
using TruckYard.Dtos;

namespace TruckYard.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Gerencia o registro de entradas e saídas (movimentações) dos caminhões.
/// </summary>
public class MovementsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MovementsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Obtém o histórico de todas as movimentações.
    /// </summary>
    /// <returns>Lista de movimentações ordenadas por data.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Movements
            .Include(m => m.Truck)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        var dtoList = list.Select(m => new MovementDto
        {
            Id = m.Id,
            TruckId = m.TruckId,
            Type = m.Type.ToString(),
            Timestamp = m.Timestamp,
            Notes = m.Notes
        });

        return Ok(dtoList);
    }

    /// <summary>
    /// Registra a entrada de um caminhão no pátio.
    /// Atualiza o status do caminhão para 'InYard'.
    /// </summary>
    /// <param name="dto">Dados da entrada.</param>
    /// <returns>Dados da movimentação registrada.</returns>
    [HttpPost("entry")]
    public async Task<IActionResult> RegisterEntry([FromBody] MovementEntryDto dto)
    {
        var truck = await _db.Trucks.FindAsync(dto.TruckId);
        if (truck == null) return NotFound("Truck not found");

        truck.IsInYard = true;
        truck.Status = TruckStatus.InYard;
        truck.LastUpdate = DateTime.UtcNow;

        var movement = new Movement
        {
            TruckId = truck.Id,
            Type = MovementType.Entry,
            Notes = dto.Notes,
            Timestamp = DateTime.UtcNow
        };

        _db.Movements.Add(movement);
        await _db.SaveChangesAsync();

        var result = new MovementDto
        {
            Id = movement.Id,
            TruckId = movement.TruckId,
            Type = movement.Type.ToString(),
            Timestamp = movement.Timestamp,
            Notes = movement.Notes
        };

        return Ok(result);
    }

    /// <summary>
    /// Registra a saída de um caminhão do pátio.
    /// Atualiza o status do caminhão para 'InTransit'.
    /// </summary>
    /// <param name="dto">Dados da saída.</param>
    /// <returns>Dados da movimentação registrada.</returns>
    [HttpPost("exit")]
    public async Task<IActionResult> RegisterExit([FromBody] MovementExitDto dto)
    {
        var truck = await _db.Trucks.FindAsync(dto.TruckId);
        if (truck == null) return NotFound("Truck not found");

        truck.IsInYard = false;
        truck.Status = TruckStatus.InTransit;
        truck.LastUpdate = DateTime.UtcNow;

        var movement = new Movement
        {
            TruckId = truck.Id,
            Type = MovementType.Exit,
            Notes = dto.Notes,
            Timestamp = DateTime.UtcNow
        };

        _db.Movements.Add(movement);
        await _db.SaveChangesAsync();

        var result = new MovementDto
        {
            Id = movement.Id,
            TruckId = movement.TruckId,
            Type = movement.Type.ToString(),
            Timestamp = movement.Timestamp,
            Notes = movement.Notes
        };

        return Ok(result);
    }
}
