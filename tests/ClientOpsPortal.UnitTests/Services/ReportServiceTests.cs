using AutoBogus;
using ClientOpsPortal.Application.DTOs.Reports;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Domain.Models.Reports;
using Moq;
using Shouldly;
using System.Globalization;
using System.Text;

namespace ClientOpsPortal.UnitTests.Services;

public class ReportsServiceTests
{
    private readonly Mock<IReportsRepository> _repositoryMock;
    private readonly ReportsService _sut;

    public ReportsServiceTests()
    {
        _repositoryMock = new Mock<IReportsRepository>();
        _sut = new ReportsService(_repositoryMock.Object);
    }

    #region GetServicesStatusAsync Tests

    [Fact]
    public async Task GetServicesStatusAsync_WhenDataExists_ReturnsServiceStatusReports()
    {
        // Arrange
        var readModels = CreateServiceStatusReadModelList(5);
        var expectedCount = readModels.Count;

        _repositoryMock
            .Setup(r => r.GetServicesWithStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(readModels);

        // Act
        var result = await _sut.GetServicesStatusAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(expectedCount);

        var firstResult = result.First();
        firstResult.ServiceId.ShouldBe(readModels.First().ServiceId);
        firstResult.ServiceName.ShouldBe(readModels.First().ServiceName);

        _repositoryMock.Verify(
            r => r.GetServicesWithStatsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetServicesStatusAsync_WhenNoDataExists_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<ServiceStatusReadModel>();

        _repositoryMock
            .Setup(r => r.GetServicesWithStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

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
        var totalCount = 10;

        _repositoryMock
            .Setup(r => r.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((subscriptions, totalCount));

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
        result.Items.Count().ShouldBe(subscriptions.Count);
        result.TotalCount.ShouldBe(totalCount);
        result.Page.ShouldBe(page);
        result.PageSize.ShouldBe(pageSize);

        _repositoryMock.Verify(
            r => r.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WithCustomParameters_ReturnsPaginationDto()
    {
        // Arrange
        var page = 2;
        var pageSize = 25;
        var subscriptions = CreateSubscriptionEntityList(3);
        var totalCount = 10;

        _repositoryMock
            .Setup(r => r.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((subscriptions, totalCount));

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.ShouldNotBeNull();
        result.Page.ShouldBe(page);
        result.PageSize.ShouldBe(pageSize);

        _repositoryMock.Verify(
            r => r.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenPageIsLessThan1_SetsPageToOne()
    {
        // Arrange
        var page = 0;
        var pageSize = 50;
        var subscriptions = CreateSubscriptionEntityList(3);
        var totalCount = 10;

        _repositoryMock
            .Setup(r => r.GetActiveSubscriptionsAsync(1, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((subscriptions, totalCount));

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.Page.ShouldBe(1);

        _repositoryMock.Verify(
            r => r.GetActiveSubscriptionsAsync(1, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenPageSizeIsLessThan1_SetsPageSizeToOne()
    {
        // Arrange
        var page = 1;
        var pageSize = 0;
        var subscriptions = CreateSubscriptionEntityList(3);
        var totalCount = 10;

        _repositoryMock
            .Setup(r => r.GetActiveSubscriptionsAsync(page, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((subscriptions, totalCount));

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.PageSize.ShouldBe(1);

        _repositoryMock.Verify(
            r => r.GetActiveSubscriptionsAsync(page, 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenPageSizeIsGreaterThan200_SetsPageSizeTo200()
    {
        // Arrange
        var page = 1;
        var pageSize = 300;
        var subscriptions = CreateSubscriptionEntityList(3);
        var totalCount = 10;

        _repositoryMock
            .Setup(r => r.GetActiveSubscriptionsAsync(page, 200, It.IsAny<CancellationToken>()))
            .ReturnsAsync((subscriptions, totalCount));

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync(page, pageSize);

        // Assert
        result.PageSize.ShouldBe(200);

        _repositoryMock.Verify(
            r => r.GetActiveSubscriptionsAsync(page, 200, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WhenNoDataExists_ReturnsEmptyPaginationDto()
    {
        // Arrange
        var page = 1;
        var pageSize = 50;
        var subscriptions = new List<Subscription>();
        var totalCount = 0;

        _repositoryMock
            .Setup(r => r.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((subscriptions, totalCount));

        // Act
        var result = await _sut.GetActiveSubscriptionsAsync();

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    #endregion

    #region GetSubscriptionsDynamicsAsync Tests

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WithCustomDateRange_ReturnsDynamicsReport()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto
        {
            DateFrom = DateTimeOffset.UtcNow.AddMonths(-6),
            DateTo = DateTimeOffset.UtcNow
        };
        var subscriptions = CreateSubscriptionEntityList(5);

        _repositoryMock
            .Setup(r => r.GetSubscriptionsForDynamicsAsync(
                filter.DateFrom.Value,
                filter.DateTo.Value,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();
        result.PeriodStart.ShouldBe(filter.DateFrom.Value);
        result.PeriodEnd.ShouldBe(filter.DateTo.Value);

        _repositoryMock.Verify(
            r => r.GetSubscriptionsForDynamicsAsync(
                filter.DateFrom.Value,
                filter.DateTo.Value,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WithServiceIdFilter_ReturnsDynamicsReport()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto
        {
            ServiceId = Guid.NewGuid()
        };
        var subscriptions = CreateSubscriptionEntityList(5);
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-1);
        var dateTo = DateTimeOffset.UtcNow;

        _repositoryMock
            .Setup(r => r.GetSubscriptionsForDynamicsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                filter.ServiceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();

        _repositoryMock.Verify(
            r => r.GetSubscriptionsForDynamicsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                filter.ServiceId,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsDynamicsAsync_WithTariffPlanIdFilter_ReturnsDynamicsReport()
    {
        // Arrange
        var filter = new DynamicsReportFilterDto
        {
            TariffPlanId = Guid.NewGuid()
        };
        var subscriptions = CreateSubscriptionEntityList(5);
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-1);
        var dateTo = DateTimeOffset.UtcNow;

        _repositoryMock
            .Setup(r => r.GetSubscriptionsForDynamicsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                null,
                filter.TariffPlanId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsDynamicsAsync(filter);

        // Assert
        result.ShouldNotBeNull();

        _repositoryMock.Verify(
            r => r.GetSubscriptionsForDynamicsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                null,
                filter.TariffPlanId,
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        var subscriptions = CreateSubscriptionEntityList(3);
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-1);
        var dateTo = DateTimeOffset.UtcNow;

        foreach (var subscription in subscriptions)
        {
            if (subscription.TariffPlan != null)
                subscription.TariffPlan.Price = 100m;
            subscription.BeginDate = dateFrom.AddDays(1);
        }

        _repositoryMock
            .Setup(r => r.GetSubscriptionsForDynamicsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

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
        var subscriptions = CreateSubscriptionEntityList(3);
        var dateFrom = DateTimeOffset.UtcNow.AddMonths(-1);
        var dateTo = DateTimeOffset.UtcNow;

        var abonentId = Guid.NewGuid();
        foreach (var subscription in subscriptions)
        {
            if (subscription.Contract != null)
                subscription.Contract.AbonentId = abonentId;
            subscription.BeginDate = dateFrom.AddDays(1);
        }

        _repositoryMock
            .Setup(r => r.GetSubscriptionsForDynamicsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

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
        lines.Length.ShouldBe(1); // Только заголовок
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

    private static List<ServiceStatusReadModel> CreateServiceStatusReadModelList(int count)
    {
        var list = new List<ServiceStatusReadModel>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new AutoFaker<ServiceStatusReadModel>().Generate());
        }
        return list;
    }

    private static List<Subscription> CreateSubscriptionEntityList(int count)
    {
        var list = new List<Subscription>();
        for (int i = 0; i < count; i++)
        {
            var subscription = new AutoFaker<Subscription>().Generate();

            subscription.Contract = new AutoFaker<Contract>().Generate();
            subscription.Contract.Abonent = new AutoFaker<Abonent>().Generate();
            subscription.Service = new AutoFaker<Service>().Generate();
            subscription.TariffPlan = new AutoFaker<TariffPlan>().Generate();

            subscription.BeginDate = DateTimeOffset.UtcNow.AddDays(-new Random().Next(1, 30));
            subscription.EndDate = new Random().Next(0, 2) == 0 ? null : DateTimeOffset.UtcNow.AddDays(new Random().Next(1, 30));

            list.Add(subscription);
        }
        return list;
    }

    private static List<ServiceStatusReportDto> CreateServiceStatusReportDtoList(int count)
    {
        var list = new List<ServiceStatusReportDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new AutoFaker<ServiceStatusReportDto>().Generate());
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
}