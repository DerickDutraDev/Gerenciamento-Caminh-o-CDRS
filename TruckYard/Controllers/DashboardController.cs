using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckYard.Data;

namespace TruckYard.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Fornece dados consolidados para o painel de controle (Dashboard).
/// </summary>
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    /// <summary>
    /// Obtém contagens e estatísticas gerais dos caminhões.
    /// </summary>
    /// <returns>Objeto contendo totais por status.</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var totalTrucks = await _db.Trucks.CountAsync();
        var inYard = await _db.Trucks.CountAsync(t => t.IsInYard);
        var loading = await _db.Trucks.CountAsync(t => t.Status == Models.TruckStatus.Loading);
        var inTransit = await _db.Trucks.CountAsync(t => t.Status == Models.TruckStatus.InTransit);
        var maintenance = await _db.Trucks.CountAsync(t => t.Status == Models.TruckStatus.Maintenance);

        return Ok(new
        {
            totalTrucks,
            inYard,
            loading,
            inTransit,
            maintenance
        });
    }
}
