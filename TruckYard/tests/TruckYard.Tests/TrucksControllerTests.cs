using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckYard.Controllers;
using TruckYard.Data;
using TruckYard.Dtos;
using TruckYard.Models;
using Xunit;

namespace TruckYard.Tests;

public class TrucksControllerTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new AppDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfTrucks()
    {
        // Arrange
        var dbContext = GetDatabaseContext();
        dbContext.Trucks.Add(new Truck { Id = 1, Plate = "ABC-1234", Model = "Volvo", IsInYard = true });
        dbContext.Trucks.Add(new Truck { Id = 2, Plate = "XYZ-5678", Model = "Scania", IsInYard = false });
        await dbContext.SaveChangesAsync();

        var controller = new TrucksController(dbContext);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnTrucks = Assert.IsAssignableFrom<IEnumerable<TruckDto>>(okResult.Value);
        Assert.Equal(2, returnTrucks.Count());
    }

    [Fact]
    public async Task AddTruck_ReturnsOkResult_WhenTruckIsCreated()
    {
        // Arrange
        var dbContext = GetDatabaseContext();
        var controller = new TrucksController(dbContext);
        var newTruck = new TruckCreateDto
        {
            Plate = "TEST-9999",
            Model = "Mercedes",
            DriverName = "João Silva",
            IsInYard = true,
            Status = "Available"
        };

        // Act
        var result = await controller.AddTruck(newTruck);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var truckInDb = await dbContext.Trucks.FirstOrDefaultAsync(t => t.Plate == "TEST-9999");
        Assert.NotNull(truckInDb);
        Assert.Equal("Mercedes", truckInDb.Model);
    }

    [Fact]
    public async Task GetYard_ReturnsOnlyTrucksInYard()
    {
        // Arrange
        var dbContext = GetDatabaseContext();
        dbContext.Trucks.Add(new Truck { Id = 1, Plate = "IN-YARD", IsInYard = true });
        dbContext.Trucks.Add(new Truck { Id = 2, Plate = "OUT-YARD", IsInYard = false });
        await dbContext.SaveChangesAsync();

        var controller = new TrucksController(dbContext);

        // Act
        var result = await controller.GetYard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var trucks = Assert.IsAssignableFrom<IEnumerable<TruckDto>>(okResult.Value);
        Assert.Single(trucks);
        Assert.Equal("IN-YARD", trucks.First().Plate);
    }
}
