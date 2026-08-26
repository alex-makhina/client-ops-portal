using ClientOpsPortal.Services.Reporting.Contracts.DTOs;
using ClientOpsPortal.Services.Reporting.Contracts.Models;
using ClientOpsPortal.Services.Reporting.Data;
using ClientOpsPortal.Services.Reporting.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ClientOpsPortal.Services.Reporting.UnitTests.Services;

public class ReportsServiceTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly ReportsRepository _repository;
    private readonly ReportsService _sut;

    public ReportsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ReportsDbContext(options);
        _repository = new ReportsRepository(_context);
        _sut = new ReportsService(_repository);
    }

    #region GetServicesStatusAsync Tests

    [Fact]
    public async Task GetServicesStatusAsync_WhenDataExists_ReturnsServiceStatusReports()
    {
        // Arrange
        var services = CreateServiceEntityList(5);
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetServicesStatusAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(services.Count);

        var firstResult = result.First();
        firstResult.ServiceId.ShouldBe(services.First().Id);
        firstResult.ServiceName.ShouldBe(services.First().Name);
    }

    [Fact]
    public async Task GetServicesStatusAsync_WhenNoDataExists_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetServicesStatusAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetActiveSubscriptionsAsync Tests

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WithDefaultParameters_ReturnsPaginationDto()
    {
        // Arrange
        var page = 1;
        var pageSize = 50;
        var subscriptions = CreateSubscriptionEntityList(3);

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count().ShouldBe(subscriptions.Count);
        result.TotalCount.ShouldBe(subscriptions.Count);
        result.Page.ShouldBe(page);
        result.PageSize.ShouldBe(pageSize);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WithCustomParameters_ReturnsPaginationDto()
    {
        // Arrange
        var page = 2;
        var pageSize = 1;
        var subscriptions = CreateSubscriptionEntityList(3);

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.ShouldNotBeNull();
        result.Page.ShouldBe(page);
        result.PageSize.ShouldBe(pageSize);
        result.Items.Count().ShouldBe(pageSize);
        result.TotalCount.ShouldBe(subscriptions.Count);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenPageIsLessThan1_SetsPageToOne()
    {
        // Arrange
        var page = 0;
        var pageSize = 50;
        var subscriptions = CreateSubscriptionEntityList(3);

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.Page.ShouldBe(1);
        result.Items.Count().ShouldBe(subscriptions.Count);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenPageSizeIsLessThan1_SetsPageSizeToOne()
    {
        // Arrange
        var page = 1;
        var pageSize = 0;
        var subscriptions = CreateSubscriptionEntityList(3);

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.PageSize.ShouldBe(1);
        result.Items.Count().ShouldBe(1);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenPageSizeIsGreaterThan200_SetsPageSizeTo200()
    {
        // Arrange
        var page = 1;
        var pageSize = 300;
        var subscriptions = CreateSubscriptionEntityList(3);

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.PageSize.ShouldBe(200);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenNoDataExists_ReturnsEmptyPaginationDto()
    {
        // Act
        var result = await _sut.GetActiveSubscriptionsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    #endregion

    #region GetSubscriptionsDynamicsAsync Tests

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WithCustomDateRange_ReturnsDynamicsReport()
    {
        // Arrange
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-6);
        var dateTo = DateTimeOffset.UtcNow;
        var filter = new DynamicsReportFilterDto
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var subscriptions = CreateSubscriptionEntityList(5);
        foreach (var subscription in subscriptions)
        {
            subscription.BeginDate = dateFrom.AddDays(1);
        }

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();
        result.PeriodStart.ShouldBe(dateFrom);
        result.PeriodEnd.ShouldBe(dateTo);
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WithServiceIdFilter_ReturnsDynamicsReport()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var filter = new DynamicsReportFilterDto
        {
            ServiceId = serviceId
        };

        var sharedService = CreateServiceEntity(serviceId);
        var matching = Enumerable.Range(0, 3)
            .Select(_ => CreateSubscriptionEntity(service: sharedService));
        var other = CreateSubscriptionEntityList(2);

        await SeedSubscriptionsAsync(matching.Concat(other));

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();
        result.NewSubscriptions.ShouldBe(3);
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WithTariffPlanIdFilter_ReturnsDynamicsReport()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var filter = new DynamicsReportFilterDto
        {
            TariffPlanId = tariffPlanId
        };

        var sharedTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var matching = Enumerable.Range(0, 3)
            .Select(_ => CreateSubscriptionEntity(tariffPlan: sharedTariffPlan));
        var other = CreateSubscriptionEntityList(2);

        await SeedSubscriptionsAsync(matching.Concat(other));

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();
        result.NewSubscriptions.ShouldBe(3);
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WhenDateFromGreaterThanDateTo_ThrowsArgumentException()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto
        {
            DateFrom = DateTimeOffset.UtcNow,
            DateTo = DateTimeOffset.UtcNow.AddMonths(-1)
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            () => _sut.GetSubscriptionsDynamicsAsync(filter));

        exception.Message.ShouldContain("DateFrom cannot be greater than DateTo");
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_CalculatesRevenueCorrectly()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto();
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-1);

        var subscriptions = CreateSubscriptionEntityList(3);
        foreach (var subscription in subscriptions)
        {
            subscription.TariffPlan!.Price = 100m;
            subscription.BeginDate = dateFrom.AddDays(1);
        }

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();
        result.TotalRevenue.ShouldBe(subscriptions.Count * 100m);
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_CalculatesUniqueAbonentsCorrectly()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto();
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-1);

        var abonentId = Guid.NewGuid();
        var sharedContract = CreateContractEntity(abonentId: abonentId);
        var subscriptions = Enumerable.Range(0, 3)
            .Select(_ => CreateSubscriptionEntity(contract: sharedContract, beginDate: dateFrom.AddDays(1)))
            .ToList();

        await SeedSubscriptionsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();
        result.UniqueAbonents.ShouldBe(1);
    }

    #endregion

    #region ExportToCsvAsync Tests

    [Fact]
    public async Task ExportToCsvAsync_WithValidData_ReturnsCsvString()
    {
        // Arrange
        var data = CreateServiceStatusReportDtoList(3);
        var reportName = "test-report";

        // Act
        var result = await _sut.ExportToCsvAsync(data, reportName);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("ServiceId");
        result.ShouldContain("ServiceName");
        result.ShouldContain("TotalSubscriptions");

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(data.Count() + 1);
    }

    [Fact]
    public async Task ExportToCsvAsync_WithEmptyData_ReturnsOnlyHeader()
    {
        // Arrange
        var data = new List<ServiceStatusReportDto>();
        var reportName = "test-report";

        // Act
        var result = await _sut.ExportToCsvAsync(data, reportName);

        // Assert
        result.ShouldNotBeNullOrEmpty();

        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(1);
        lines[0].ShouldContain("ServiceId");
    }

    [Fact]
    public async Task ExportToCsvAsync_WithDateTimeOffset_FormatsCorrectly()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 1, 15, 14, 30, 0, TimeSpan.Zero);
        var data = new List<TestExportDto>
        {
            new TestExportDto { Date = date, Name = "Test" }
        };

        // Act
        var result = await _sut.ExportToCsvAsync(data, "test-report");

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("15.01.2024 14:30:00");
    }

    [Fact]
    public async Task ExportToCsvAsync_WithSpecialCharacters_EscapesQuotes()
    {
        // Arrange
        var data = new List<TestExportDto>
        {
            new TestExportDto { Name = "Test with \"quotes\" and, commas" }
        };

        // Act
        var result = await _sut.ExportToCsvAsync(data, "test-report");

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("\"\"quotes\"\"");
    }

    #endregion

    #region Helper Methods

    private async Task SeedSubscriptionsAsync(IEnumerable<Subscription> subscriptions)
    {
        await _context.Subscriptions.AddRangeAsync(subscriptions);
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
        Guid? id = null,
        Service? service = null,
        TariffPlan? tariffPlan = null,
        Contract? contract = null,
        DateTimeOffset? beginDate = null,
        DateTimeOffset? endDate = null)
    {
        service ??= CreateServiceEntity();
        tariffPlan ??= CreateTariffPlanEntity(serviceId: service.Id);
        contract ??= CreateContractEntity();

        return new Subscription
        {
            Id = id ?? Guid.NewGuid(),
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

    private static TariffPlan CreateTariffPlanEntity(Guid? id = null, Guid? serviceId = null, decimal price = 100m)
    {
        return new TariffPlan
        {
            Id = id ?? Guid.NewGuid(),
            Name = $"Tariff {Guid.NewGuid():N}",
            Description = "Description for tariff",
            Price = price,
            ServiceId = serviceId ?? Guid.NewGuid(),
            BeginDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = null
        };
    }

    private static Abonent CreateAbonentEntity(Guid? id = null)
    {
        return new Abonent
        {
            Id = id ?? Guid.NewGuid(),
            IdentificationNumber = $"ID{Guid.NewGuid():N}",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = null,
            UserId = Guid.NewGuid(),
            AccountNumber = "ACC12345",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user"
        };
    }

    private static Contract CreateContractEntity(Guid? id = null, Guid? abonentId = null)
    {
        var abonent = CreateAbonentEntity(abonentId);
        var contract = new Contract
        {
            Id = id ?? Guid.NewGuid(),
            ContractNumber = $"C{Guid.NewGuid():N}",
            AbonentId = abonent.Id,
            BeginDate = DateTimeOffset.UtcNow.AddDays(-30),
            EndDate = null,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user",
            Subscriptions = new List<Subscription>()
        };
        contract.Abonent = abonent;
        return contract;
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

    private static List<ServiceStatusReportDto> CreateServiceStatusReportDtoList(int count)
    {
        var list = new List<ServiceStatusReportDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new ServiceStatusReportDto
            {
                ServiceId = Guid.NewGuid(),
                ServiceName = $"Service {i}",
                ServiceDescription = $"Description {i}",
                IsActive = true,
                BeginDate = DateTimeOffset.UtcNow,
                EndDate = null,
                TotalSubscriptions = i,
                ActiveSubscriptions = i,
                InactiveSubscriptions = 0,
                AverageTariffPrice = 100m
            });
        }
        return list;
    }

    private class TestExportDto
    {
        public string? Name { get; set; }
        public DateTimeOffset? Date { get; set; }
        public decimal? Price { get; set; }
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
