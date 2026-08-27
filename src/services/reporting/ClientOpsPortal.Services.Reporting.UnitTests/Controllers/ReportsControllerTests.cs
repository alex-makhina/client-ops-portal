using ClientOpsPortal.Services.Reporting.Contracts.DTOs;
using ClientOpsPortal.Services.Reporting.Contracts.Models;
using ClientOpsPortal.Services.Reporting.Controllers;
using ClientOpsPortal.Services.Reporting.Data;
using ClientOpsPortal.Services.Reporting.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ClientOpsPortal.Services.Reporting.UnitTests.Controllers;

public class ReportsControllerTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly ReportsController _sut;

    public ReportsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ReportsDbContext(options);
        var repository = new ReportsRepository(_context);
        var service = new ReportsService(repository);
        _sut = new ReportsController(service);
    }

    #region GetServicesStatus Tests

    [Fact]
    public async Task GetServicesStatus_WhenFormatIsJson_ReturnsOkWithData()
    {
        // Arrange
        var services = CreateServiceEntityList(5);
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetServicesStatus("json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceStatusReportDto>>();
        data.Count().ShouldBe(5);
    }

    [Fact]
    public async Task GetServicesStatus_WhenFormatIsCsv_ReturnsFileResult()
    {
        // Arrange
        var services = CreateServiceEntityList(3);
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetServicesStatus("csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
        fileResult.FileDownloadName.ShouldContain("services-status");
        fileResult.FileDownloadName.ShouldEndWith(".csv");
    }

    [Fact]
    public async Task GetServicesStatus_WhenFormatIsCsvAndDataIsEmpty_ReturnsFileResultWithEmptyCsv()
    {
        // Act
        var result = await _sut.GetServicesStatus("csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
    }

    #endregion

    #region GetActiveSubscriptions Tests

    [Fact]
    public async Task GetActiveSubscriptions_WhenFormatIsJson_ReturnsOkWithPaginationData()
    {
        // Arrange
        var page = 1;
        var pageSize = 50;
        var subscriptions = CreateSubscriptionEntityList(3);
        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptions(page, pageSize, "json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeOfType<ReportPaginationDto<ActiveSubscriptionReportDto>>();

        data.ShouldNotBeNull();
        data.Items.Count().ShouldBe(subscriptions.Count);
        data.TotalCount.ShouldBe(subscriptions.Count);
    }

    [Fact]
    public async Task GetActiveSubscriptions_WhenFormatIsCsv_ReturnsFileResultWithItems()
    {
        // Arrange
        var page = 1;
        var pageSize = 50;
        var subscriptions = CreateSubscriptionEntityList(3);
        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptions(page, pageSize, "csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
        fileResult.FileDownloadName.ShouldContain("active-subscriptions");
        fileResult.FileDownloadName.ShouldEndWith(".csv");
    }

    [Fact]
    public async Task GetActiveSubscriptions_WithCustomPageAndPageSize_PassesParametersToService()
    {
        // Arrange
        var page = 2;
        var pageSize = 25;
        var subscriptions = CreateSubscriptionEntityList(3);
        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptions(page, pageSize, "json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeOfType<ReportPaginationDto<ActiveSubscriptionReportDto>>();
        data.Page.ShouldBe(page);
        data.PageSize.ShouldBe(pageSize);
    }

    #endregion

    #region GetSubscriptionsDynamics Tests

    [Fact]
    public async Task GetSubscriptionsDynamics_WhenFormatIsJson_ReturnsOkWithData()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto();
        var subscriptions = CreateSubscriptionEntityList(3);
        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamics(filter, "json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeOfType<SubscriptionDynamicsReportDto>();
        data.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetSubscriptionsDynamics_WhenFormatIsCsv_ReturnsFileResultWithData()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto();
        var subscriptions = CreateSubscriptionEntityList(3);
        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamics(filter, "csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
        fileResult.FileDownloadName.ShouldContain("subscriptions-dynamics");
        fileResult.FileDownloadName.ShouldEndWith(".csv");
    }

    [Fact]
    public async Task GetSubscriptionsDynamics_WithFilterParameters_PassesFilterToService()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto
        {
            DateFrom = DateTimeOffset.UtcNow.AddMonths(-6),
            DateTo = DateTimeOffset.UtcNow
        };
        var subscriptions = CreateSubscriptionEntityList(3);
        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamics(filter, "json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeOfType<SubscriptionDynamicsReportDto>();
        data.PeriodStart.ShouldBe(filter.DateFrom.Value);
        data.PeriodEnd.ShouldBe(filter.DateTo.Value);
    }

    #endregion

    #region CSV Export Helper Tests

    [Fact]
    public async Task ReturnCsvAsync_ShouldGenerateFileNameWithCurrentDate()
    {
        // Arrange
        var services = CreateServiceEntityList(1);
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        var expectedDatePattern = DateTime.UtcNow.ToString("yyyyMMdd");

        // Act
        var result = await _sut.GetServicesStatus("csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.FileDownloadName.ShouldContain("services-status");
        fileResult.FileDownloadName.ShouldContain(expectedDatePattern);
        fileResult.FileDownloadName.ShouldEndWith(".csv");
    }

    #endregion

    #region Helper Methods

    private async Task SeedSubscriptionsAsync(IEnumerable<Subscription> subscriptions)
    {
        var list = subscriptions.ToList();

        await _context.Abonents.AddRangeAsync(
            list.Select(s => s.Contract!.Abonent!).Where(a => a != null).DistinctBy(a => a.Id));
        await _context.Contracts.AddRangeAsync(
            list.Select(s => s.Contract!).Where(c => c != null).DistinctBy(c => c.Id));
        await _context.Services.AddRangeAsync(
            list.Select(s => s.Service!).Where(s => s != null).DistinctBy(s => s.Id));
        await _context.TariffPlans.AddRangeAsync(
            list.Select(s => s.TariffPlan!).Where(t => t != null).DistinctBy(t => t.Id));
        await _context.Subscriptions.AddRangeAsync(list);
        await _context.SaveChangesAsync();
    }

    private static Service CreateServiceEntity(Guid? id = null, DateTimeOffset? endDate = null)
    {
        var serviceId = id ?? Guid.NewGuid();
        return new Service
        {
            Id = serviceId,
            Name = $"Service {serviceId:N}",
            Description = $"Description for service {serviceId:N}",
            BeginDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = endDate,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user",
            TariffPlans = new List<TariffPlan>()
        };
    }

    private static List<Service> CreateServiceEntityList(int count)
    {
        var list = new List<Service>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateServiceEntity());
        }
        return list;
    }

    private static Subscription CreateSubscriptionEntity(
        DateTimeOffset? beginDate = null,
        DateTimeOffset? endDate = null)
    {
        var service = CreateServiceEntity();
        var tariffPlan = new TariffPlan
        {
            Id = Guid.NewGuid(),
            Name = $"Tariff {Guid.NewGuid():N}",
            Description = "Description for tariff",
            Price = 100m,
            ServiceId = service.Id,
            BeginDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = null
        };
        var abonent = new Abonent
        {
            Id = Guid.NewGuid(),
            IdentificationNumber = $"ID{Guid.NewGuid():N}",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = null,
            UserId = Guid.NewGuid(),
            AccountNumber = "ACC12345",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user"
        };
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            ContractNumber = $"C{Guid.NewGuid():N}",
            AbonentId = abonent.Id,
            BeginDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = null,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user",
            Subscriptions = new List<Subscription>()
        };
        contract.Abonent = abonent;

        return new Subscription
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            ServiceId = service.Id,
            TariffPlanId = tariffPlan.Id,
            BeginDate = beginDate ?? DateTimeOffset.UtcNow.AddDays(-5),
            EndDate = endDate,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user",
            Contract = contract,
            Service = service,
            TariffPlan = tariffPlan
        };
    }

    private static List<Subscription> CreateSubscriptionEntityList(int count)
    {
        var list = new List<Subscription>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionEntity());
        }
        return list;
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
