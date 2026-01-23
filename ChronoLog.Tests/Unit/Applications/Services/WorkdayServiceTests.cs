using ChronoLog.Applications.Services;
using ChronoLog.Core.Interfaces;
using ChronoLog.Core.Models;
using ChronoLog.Core.Models.DisplayObjects;
using ChronoLog.SqlDatabase.Context;
using ChronoLog.SqlDatabase.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ChronoLog.Tests.Unit.Applications.Services;

public class WorkdayServiceTests
{
    [Fact]
    public async Task GetOfficeDaysCountAsync_ShouldReturnCorrectCount_WhenWorkdaysExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SqlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new SqlDbContext(options);
        var employeeId = Guid.NewGuid();

        context.Workdays.AddRange(
            new WorkdayEntity
            {
                WorkdayId = Guid.NewGuid(), EmployeeId = employeeId, Date = new DateTime(2024, 1, 1),
                Type = WorkdayType.Office
            },
            new WorkdayEntity
            {
                WorkdayId = Guid.NewGuid(), EmployeeId = employeeId, Date = new DateTime(2024, 2, 1),
                Type = WorkdayType.Office
            },
            new WorkdayEntity
            {
                WorkdayId = Guid.NewGuid(), EmployeeId = employeeId, Date = new DateTime(2024, 3, 1),
                Type = WorkdayType.Homeoffice
            }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var mockEmployeeContext = new Mock<IEmployeeContextService>();
        mockEmployeeContext.Setup(x => x.GetOrCreateCurrentEmployeeAsync())
            .ReturnsAsync(new EmployeeModel { EmployeeId = employeeId });

        var service = new WorkdayService(context, mockEmployeeContext.Object);

        // Act
        var result = await service.GetOfficeDaysCountAsync(2024);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetOfficeDaysCountAsync_ShouldReturnZero_WhenNoWorkdaysExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SqlDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new SqlDbContext(options);
        var employeeId = Guid.NewGuid();

        var mockEmployeeContext = new Mock<IEmployeeContextService>();
        mockEmployeeContext.Setup(x => x.GetOrCreateCurrentEmployeeAsync()).ReturnsAsync(
            new EmployeeModel { EmployeeId = employeeId });

        var service = new WorkdayService(context, mockEmployeeContext.Object);

        // Act
        var result = await service.GetOfficeDaysCountAsync(2024);

        // Assert
        result.Should().Be(0);
    }
}