using AutoBogus;
using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Services.Directory.Client;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;

using DirectoryDto = ClientOpsPortal.Services.Directory.Contracts.DTOs;

namespace ClientOpsPortal.UnitTests.Controllers;

public class ServicesControllerTests
{
    private readonly Mock<IServicesDirectoryClient> _directoryClientMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ServicesController _sut;

    public ServicesControllerTests()
    {
        _directoryClientMock = new Mock<IServicesDirectoryClient>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(Guid.NewGuid());

        _sut = new ServicesController(
            _directoryClientMock.Object,
            _currentUserServiceMock.Object);

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenServicesExist_ReturnsOkWithServices()
    {
        // Arrange
        var expectedServices = CreateServiceDtoList(5);

        _directoryClientMock
            .Setup(c => c.GetAllServicesAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.ServiceDto>>();
        services.Count().ShouldBe(5);

        _directoryClientMock.Verify(
            c => c.GetAllServicesAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoServicesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _directoryClientMock
            .Setup(c => c.GetAllServicesAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DirectoryDto.ServiceDto>());

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.ServiceDto>>();
        services.ShouldBeEmpty();

        _directoryClientMock.Verify(
            c => c.GetAllServicesAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToClient()
    {
        // Arrange
        var expectedServices = CreateServiceDtoList(3);

        _directoryClientMock
            .Setup(c => c.GetAllServicesAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _directoryClientMock.Verify(
            c => c.GetAllServicesAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenServiceExists_ReturnsOkWithService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedService = CreateServiceDto(serviceId);

        _directoryClientMock
            .Setup(c => c.GetServiceByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedService);

        // Act
        var result = await _sut.GetById(serviceId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var service = okResult.Value.ShouldBeOfType<DirectoryDto.ServiceDto>();
        service.Id.ShouldBe(serviceId);
        service.Name.ShouldBe(expectedService.Name);

        _directoryClientMock.Verify(
            c => c.GetServiceByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _directoryClientMock
            .Setup(c => c.GetServiceByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DirectoryDto.ServiceDto?)null);

        // Act
        var result = await _sut.GetById(serviceId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(serviceId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _directoryClientMock.Verify(
            c => c.GetServiceByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToClient()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedService = CreateServiceDto(serviceId);

        _directoryClientMock
            .Setup(c => c.GetServiceByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedService);

        // Act
        var result = await _sut.GetById(serviceId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _directoryClientMock.Verify(
            c => c.GetServiceByIdAsync(serviceId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetFullServiceData Tests

    [Fact]
    public async Task GetFullServiceData_WhenServiceExists_ReturnsOkWithFullServiceData()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedService = CreateFullServiceDto(serviceId);

        _directoryClientMock
            .Setup(c => c.GetFullServiceDataAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedService);

        // Act
        var result = await _sut.GetFullServiceData(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var service = okResult.Value.ShouldBeOfType<DirectoryDto.ServiceFullDataDto>();
        service.Id.ShouldBe(serviceId);
        service.Name.ShouldBe(expectedService.Name);

        _directoryClientMock.Verify(
            c => c.GetFullServiceDataAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFullServiceData_WhenServiceNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _directoryClientMock
            .Setup(c => c.GetFullServiceDataAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DirectoryDto.ServiceFullDataDto?)null);

        // Act
        var result = await _sut.GetFullServiceData(serviceId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(serviceId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _directoryClientMock.Verify(
            c => c.GetFullServiceDataAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveServices Tests

    [Fact]
    public async Task GetActiveServices_WhenServicesExist_ReturnsOkWithActiveServices()
    {
        // Arrange
        var expectedServices = CreateServiceShortDataDtoList(3);

        _directoryClientMock
            .Setup(c => c.GetActiveServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetActiveServices();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.ServiceShortDataDto>>();
        services.Count().ShouldBe(3);

        _directoryClientMock.Verify(
            c => c.GetActiveServicesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveServices_WhenNoServicesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _directoryClientMock
            .Setup(c => c.GetActiveServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DirectoryDto.ServiceShortDataDto>());

        // Act
        var result = await _sut.GetActiveServices();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.ServiceShortDataDto>>();
        services.ShouldBeEmpty();

        _directoryClientMock.Verify(
            c => c.GetActiveServicesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdService = CreateServiceDto(Guid.NewGuid());
        createdService.Name = createDto.Name;

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.CreateServiceAsync(It.IsAny<DirectoryDto.CreateServiceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdService);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(ServicesController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdService.Id);
        createdResult.Value.ShouldBeOfType<DirectoryDto.ServiceDto>().ShouldBe(createdService);

        _directoryClientMock.Verify(
            c => c.CreateServiceAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var errorMessage = "Service with same name already exists";

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.CreateServiceAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _directoryClientMock.Verify(
            c => c.CreateServiceAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenValidDto_ReturnsOkWithUpdatedService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();
        var updatedService = CreateServiceDto(serviceId);
        updatedService.Name = updateDto.Name;

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.UpdateServiceAsync(serviceId, It.IsAny<DirectoryDto.UpdateServiceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedService);

        // Act
        var result = await _sut.Update(serviceId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var service = okResult.Value.ShouldBeOfType<DirectoryDto.ServiceDto>();
        service.Id.ShouldBe(serviceId);
        service.Name.ShouldBe(updatedService.Name);

        _directoryClientMock.Verify(
            c => c.UpdateServiceAsync(serviceId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenServiceNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.UpdateServiceAsync(serviceId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Not found", null, System.Net.HttpStatusCode.NotFound));

        // Act
        var result = await _sut.Update(serviceId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(serviceId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _directoryClientMock.Verify(
            c => c.UpdateServiceAsync(serviceId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenOtherExceptionThrown_ThrowsException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();
        var errorMessage = "Validation error";

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.UpdateServiceAsync(serviceId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            () => _sut.Update(serviceId, updateDto));

        exception.Message.ShouldBe(errorMessage);

        _directoryClientMock.Verify(
            c => c.UpdateServiceAsync(serviceId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenServiceExists_ReturnsNoContent()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.DeleteServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(serviceId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _directoryClientMock.Verify(
            c => c.DeleteServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenServiceNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.DeleteServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Not found", null, System.Net.HttpStatusCode.NotFound));

        // Act
        var result = await _sut.Delete(serviceId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(serviceId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _directoryClientMock.Verify(
            c => c.DeleteServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenOtherExceptionThrown_ThrowsException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var errorMessage = "Database error";

        SetupUserRole("ServiceManager");

        _directoryClientMock
            .Setup(c => c.DeleteServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            () => _sut.Delete(serviceId));

        exception.Message.ShouldBe(errorMessage);

        _directoryClientMock.Verify(
            c => c.DeleteServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupUserRole(string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    private static DirectoryDto.ServiceDto CreateServiceDto(Guid? id = null)
    {
        var faker = new AutoFaker<DirectoryDto.ServiceDto>();

        if (id.HasValue)
            faker.RuleFor(dto => dto.Id, _ => id.Value);

        return faker
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<DirectoryDto.ServiceDto> CreateServiceDtoList(int count)
    {
        var list = new List<DirectoryDto.ServiceDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateServiceDto());
        }
        return list;
    }

    private static DirectoryDto.ServiceFullDataDto CreateFullServiceDto(Guid? id = null)
    {
        var faker = new AutoFaker<DirectoryDto.ServiceFullDataDto>();

        if (id.HasValue)
            faker.RuleFor(dto => dto.Id, _ => id.Value);

        return faker
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<DirectoryDto.TariffPlanDto>())
            .Generate();
    }

    private static DirectoryDto.ServiceShortDataDto CreateServiceShortDataDto()
    {
        var faker = new AutoFaker<DirectoryDto.ServiceShortDataDto>();

        return faker
            .RuleFor(dto => dto.Id, f => f.Random.Guid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<DirectoryDto.ServiceShortDataDto> CreateServiceShortDataDtoList(int count)
    {
        var list = new List<DirectoryDto.ServiceShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateServiceShortDataDto());
        }
        return list;
    }

    private static DirectoryDto.CreateServiceDto CreateCreateServiceDto()
    {
        return new AutoFaker<DirectoryDto.CreateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<DirectoryDto.CreateTariffPlanDto>())
            .Generate();
    }

    private static DirectoryDto.UpdateServiceDto CreateUpdateServiceDto()
    {
        return new AutoFaker<DirectoryDto.UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<DirectoryDto.UpdateTariffPlanFromServiceDto>())
            .Generate();
    }

    #endregion
}