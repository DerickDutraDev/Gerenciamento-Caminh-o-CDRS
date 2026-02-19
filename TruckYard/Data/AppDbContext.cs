using Microsoft.EntityFrameworkCore;
using TruckYard.Models;

namespace TruckYard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext (DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<Nfe> Nfes => Set<Nfe>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Truck>()
            .HasIndex(t => t.Plate)
            .IsUnique();
    }
}