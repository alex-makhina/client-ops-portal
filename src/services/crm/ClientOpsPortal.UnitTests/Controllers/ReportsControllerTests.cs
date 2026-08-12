using AutoBogus;
using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.DTOs.Reports;
using ClientOpsPortal.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Text;

namespace ClientOpsPortal.UnitTests.Controllers;

public class ReportsControllerTests
{
    private readonly Mock<IReportsService> _reportsServiceMock;
    private readonly ReportsController _sut;

    public ReportsControllerTests()
    {
        _reportsServiceMock = new Mock<IReportsService>();
        _sut = new ReportsController(_reportsServiceMock.Object);
    }

    #region GetServicesStatus Tests

    [Fact]
    public async Task GetServicesStatus_WhenFormatIsJson_ReturnsOkWithData()
    {
        // Arrange
        var expectedData = CreateServiceStatusReportDtoList(5);

        _reportsServiceMock
            .Setup(s => s.GetServicesStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetServicesStatus("json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceStatusReportDto>>();
        data.Count().ShouldBe(5);

        _reportsServiceMock.Verify(
            s => s.GetServicesStatusAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetServicesStatus_WhenFormatIsCsv_ReturnsFileResult()
    {
        // Arrange
        var expectedData = CreateServiceStatusReportDtoList(3);
        var csvContent = "Name,Status,Count\nService1,Active,10\nService2,Inactive,5";
        var reportName = "services-status";

        _reportsServiceMock
            .Setup(s => s.GetServicesStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        _reportsServiceMock
            .Setup(s => s.ExportToCsvAsync(expectedData, reportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvContent);

        // Act
        var result = await _sut.GetServicesStatus("csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>(); // Исправлено на FileContentResult
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
        fileResult.FileDownloadName.ShouldContain(reportName);
        fileResult.FileDownloadName.ShouldEndWith(".csv");

        _reportsServiceMock.Verify(
            s => s.GetServicesStatusAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _reportsServiceMock.Verify(
            s => s.ExportToCsvAsync(expectedData, reportName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetServicesStatus_WhenFormatIsCsvAndDataIsEmpty_ReturnsFileResultWithEmptyCsv()
    {
        // Arrange
        var expectedData = new List<ServiceStatusReportDto>();
        var csvContent = "";
        var reportName = "services-status";

        _reportsServiceMock
            .Setup(s => s.GetServicesStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        _reportsServiceMock
            .Setup(s => s.ExportToCsvAsync(expectedData, reportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvContent);

        // Act
        var result = await _sut.GetServicesStatus("csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>(); // Исправлено на FileContentResult
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");

        _reportsServiceMock.Verify(
            s => s.ExportToCsvAsync(expectedData, reportName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveSubscriptions Tests

    [Fact]
    public async Task GetActiveSubscriptions_WhenFormatIsJson_ReturnsOkWithPaginationData()
    {
        // Arrange
        var page = 1;
        var pageSize = 50;
        var expectedData = CreateActiveSubscriptionReportPaginationDto();

        _reportsServiceMock
            .Setup(s => s.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetActiveSubscriptions(page, pageSize, "json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeOfType<ReportPaginationDto<ActiveSubscriptionReportDto>>();

        data.ShouldNotBeNull();
        data.Items.ShouldBe(expectedData.Items);
        data.TotalCount.ShouldBe(expectedData.TotalCount);

        _reportsServiceMock.Verify(
            s => s.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptions_WhenFormatIsCsv_ReturnsFileResultWithItems()
    {
        // Arrange
        var page = 1;
        var pageSize = 50;
        var expectedData = CreateActiveSubscriptionReportPaginationDto();
        var csvContent = "Abonent,Service,StartDate\nJohn Doe,Internet,2024-01-01";
        var reportName = "active-subscriptions";

        _reportsServiceMock
            .Setup(s => s.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        _reportsServiceMock
            .Setup(s => s.ExportToCsvAsync(expectedData.Items, reportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvContent);

        // Act
        var result = await _sut.GetActiveSubscriptions(page, pageSize, "csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>(); // Исправлено на FileContentResult
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
        fileResult.FileDownloadName.ShouldContain(reportName);
        fileResult.FileDownloadName.ShouldEndWith(".csv");

        _reportsServiceMock.Verify(
            s => s.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);

        _reportsServiceMock.Verify(
            s => s.ExportToCsvAsync(expectedData.Items, reportName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptions_WithCustomPageAndPageSize_PassesParametersToService()
    {
        // Arrange
        var page = 2;
        var pageSize = 25;
        var expectedData = CreateActiveSubscriptionReportPaginationDto();

        _reportsServiceMock
            .Setup(s => s.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetActiveSubscriptions(page, pageSize, "json");

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _reportsServiceMock.Verify(
            s => s.GetActiveSubscriptionsAsync(page, pageSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetSubscriptionsDynamics Tests

    [Fact]
    public async Task GetSubscriptionsDynamics_WhenFormatIsJson_ReturnsOkWithData()
    {
        // Arrange
        var filter = CreateDynamicsReportFilterDto();
        var expectedData = CreateSubscriptionDynamicsReportDto();

        _reportsServiceMock
            .Setup(s => s.GetSubscriptionsDynamicsAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetSubscriptionsDynamics(filter, "json");

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var data = okResult.Value.ShouldBeOfType<SubscriptionDynamicsReportDto>();

        data.ShouldNotBeNull();

        _reportsServiceMock.Verify(
            s => s.GetSubscriptionsDynamicsAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsDynamics_WhenFormatIsCsv_ReturnsFileResultWithData()
    {
        // Arrange
        var filter = CreateDynamicsReportFilterDto();
        var expectedData = CreateSubscriptionDynamicsReportDto();
        var csvContent = "Period,NewSubscriptions,CancelledSubscriptions,ActiveSubscriptions\n2024-01,100,20,500";
        var reportName = "subscriptions-dynamics";

        _reportsServiceMock
            .Setup(s => s.GetSubscriptionsDynamicsAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        _reportsServiceMock
            .Setup(s => s.ExportToCsvAsync(new[] { expectedData }, reportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvContent);

        // Act
        var result = await _sut.GetSubscriptionsDynamics(filter, "csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("text/csv; charset=utf-8");
        fileResult.FileDownloadName.ShouldContain(reportName);
        fileResult.FileDownloadName.ShouldEndWith(".csv");

        _reportsServiceMock.Verify(
            s => s.GetSubscriptionsDynamicsAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);

        _reportsServiceMock.Verify(
            s => s.ExportToCsvAsync(new[] { expectedData }, reportName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsDynamics_WithFilterParameters_PassesFilterToService()
    {
        // Arrange
        var filter = CreateDynamicsReportDtoWithSpecificValues();
        var expectedData = CreateSubscriptionDynamicsReportDto();

        _reportsServiceMock
            .Setup(s => s.GetSubscriptionsDynamicsAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetSubscriptionsDynamics(filter, "json");

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _reportsServiceMock.Verify(
            s => s.GetSubscriptionsDynamicsAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CSV Export Helper Tests

    [Fact]
    public async Task ReturnCsvAsync_ShouldGenerateFileNameWithCurrentDate()
    {
        // Arrange
        var expectedData = CreateServiceStatusReportDtoList(1);
        var reportName = "services-status";
        var csvContent = "Test,Data\n1,2";
        var expectedDatePattern = DateTime.UtcNow.ToString("yyyyMMdd");

        _reportsServiceMock
            .Setup(s => s.GetServicesStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        _reportsServiceMock
            .Setup(s => s.ExportToCsvAsync(expectedData, reportName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvContent);

        // Act
        var result = await _sut.GetServicesStatus("csv");

        // Assert
        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.FileDownloadName.ShouldContain(reportName);
        fileResult.FileDownloadName.ShouldContain(expectedDatePattern);
        fileResult.FileDownloadName.ShouldEndWith(".csv");
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<ServiceStatusReportDto> CreateServiceStatusReportDtoList(int count)
    {
        var list = new List<ServiceStatusReportDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new AutoFaker<ServiceStatusReportDto>().Generate());
        }
        return list;
    }

    private static ReportPaginationDto<ActiveSubscriptionReportDto> CreateActiveSubscriptionReportPaginationDto()
    {
        var items = new List<ActiveSubscriptionReportDto>();
        for (int i = 0; i < 3; i++)
        {
            items.Add(new AutoFaker<ActiveSubscriptionReportDto>().Generate());
        }

        return new AutoFaker<ReportPaginationDto<ActiveSubscriptionReportDto>>()
            .RuleFor(dto => dto.Items, _ => items)
            .RuleFor(dto => dto.TotalCount, _ => items.Count)
            .Generate();
    }

    private static DynamicsReportFilterDto CreateDynamicsReportFilterDto()
    {
        return new AutoFaker<DynamicsReportFilterDto>().Generate();
    }

    private static DynamicsReportFilterDto CreateDynamicsReportDtoWithSpecificValues()
    {
        var faker = new AutoFaker<DynamicsReportFilterDto>();
        return faker
            .RuleFor(dto => dto.DateFrom, _ => DateTimeOffset.UtcNow.AddMonths(-6))
            .RuleFor(dto => dto.DateTo, _ => DateTimeOffset.UtcNow)
            .Generate();
    }

    private static SubscriptionDynamicsReportDto CreateSubscriptionDynamicsReportDto()
    {
        return new AutoFaker<SubscriptionDynamicsReportDto>().Generate();
    }

    #endregion
}