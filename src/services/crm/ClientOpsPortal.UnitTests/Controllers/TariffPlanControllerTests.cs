using AutoBogus;
using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Services.Directory.Client;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using Shouldly;
using System.Security.Claims;
using System.Security.Principal;

using DirectoryDto = ClientOpsPortal.Services.Directory.Contracts.DTOs;

namespace ClientOpsPortal.UnitTests.Controllers;

public class TariffPlansControllerTests : IDisposable
{
    private readonly Mock<IServicesDirectoryClient> _directoryClientMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TariffPlansController _sut;

    public TariffPlansControllerTests()
    {
        _directoryClientMock = new Mock<IServicesDirectoryClient>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(Guid.NewGuid());

        _sut = new TariffPlansController(
            _directoryClientMock.Object,
            _currentUserServiceMock.Object);

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });
    }

    public void Dispose()
    {
        _directoryClientMock.Reset();
        _currentUserServiceMock.Reset();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenTariffsExist_ReturnsOkWithTariffs()
    {
        // Arrange
        var expectedTariffs = CreateTariffPlanDtoList(5);

        _directoryClientMock
            .Setup(c => c.GetAllTariffPlansAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffs);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.TariffPlanDto>>();
        tariffs.Count().ShouldBe(5);

        _directoryClientMock.Verify(
            c => c.GetAllTariffPlansAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoTariffsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _directoryClientMock
            .Setup(c => c.GetAllTariffPlansAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DirectoryDto.TariffPlanDto>());

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.TariffPlanDto>>();
        tariffs.ShouldBeEmpty();

        _directoryClientMock.Verify(
            c => c.GetAllTariffPlansAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToClient()
    {
        // Arrange
        var expectedTariffs = CreateTariffPlanDtoList(3);

        _directoryClientMock
            .Setup(c => c.GetAllTariffPlansAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffs);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _directoryClientMock.Verify(
            c => c.GetAllTariffPlansAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenTariffExists_ReturnsOkWithTariff()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var expectedTariff = CreateTariffPlanDto(tariffId);

        _directoryClientMock
            .Setup(c => c.GetTariffPlanByIdAsync(tariffId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariff);

        // Act
        var result = await _sut.GetById(tariffId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariff = okResult.Value.ShouldBeOfType<DirectoryDto.TariffPlanDto>();
        tariff.Id.ShouldBe(tariffId);
        tariff.Name.ShouldBe(expectedTariff.Name);

        _directoryClientMock.Verify(
            c => c.GetTariffPlanByIdAsync(tariffId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenTariffNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var tariffId = Guid.NewGuid();

        _directoryClientMock
            .Setup(c => c.GetTariffPlanByIdAsync(tariffId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DirectoryDto.TariffPlanDto?)null);

        // Act
        var result = await _sut.GetById(tariffId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(tariffId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _directoryClientMock.Verify(
            c => c.GetTariffPlanByIdAsync(tariffId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToClient()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var expectedTariff = CreateTariffPlanDto(tariffId);

        _directoryClientMock
            .Setup(c => c.GetTariffPlanByIdAsync(tariffId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariff);

        // Act
        var result = await _sut.GetById(tariffId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _directoryClientMock.Verify(
            c => c.GetTariffPlanByIdAsync(tariffId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByService Tests

    [Fact]
    public async Task GetByService_WhenTariffsExist_ReturnsOkWithTariffs()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedTariffs = CreateTariffPlanDtoList(3);

        _directoryClientMock
            .Setup(c => c.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffs);

        // Act
        var result = await _sut.GetByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.TariffPlanDto>>();
        tariffs.Count().ShouldBe(3);

        _directoryClientMock.Verify(
            c => c.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByService_WhenNoTariffsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _directoryClientMock
            .Setup(c => c.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DirectoryDto.TariffPlanDto>());

        // Act
        var result = await _sut.GetByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.TariffPlanDto>>();
        tariffs.ShouldBeEmpty();

        _directoryClientMock.Verify(
            c => c.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveByService Tests

    [Fact]
    public async Task GetActiveByService_WhenTariffsExist_ReturnsOkWithActiveTariffs()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedTariffs = CreateTariffPlanShortDataDtoList(3);

        _directoryClientMock
            .Setup(c => c.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffs);

        // Act
        var result = await _sut.GetActiveByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.TariffPlanShortDataDto>>();
        tariffs.Count().ShouldBe(3);

        _directoryClientMock.Verify(
            c => c.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveByService_WhenNoTariffsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _directoryClientMock
            .Setup(c => c.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DirectoryDto.TariffPlanShortDataDto>());

        // Act
        var result = await _sut.GetActiveByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<DirectoryDto.TariffPlanShortDataDto>>();
        tariffs.ShouldBeEmpty();

        _directoryClientMock.Verify(
            c => c.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithTariff()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdTariff = CreateTariffPlanDto(Guid.NewGuid());
        createdTariff.Name = createDto.Name;
        createdTariff.Price = createDto.Price;
        createdTariff.ServiceId = createDto.ServiceId;

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.CreateTariffPlanAsync(It.IsAny<DirectoryDto.CreateTariffPlanDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTariff);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(TariffPlansController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdTariff.Id);
        createdResult.Value.ShouldBeOfType<DirectoryDto.TariffPlanDto>().ShouldBe(createdTariff);

        _directoryClientMock.Verify(
            c => c.CreateTariffPlanAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var errorMessage = "Tariff plan with same name already exists";

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.CreateTariffPlanAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _directoryClientMock.Verify(
            c => c.CreateTariffPlanAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenValidDto_ReturnsOkWithUpdatedTariff()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();
        var updatedTariff = CreateTariffPlanDto(tariffId);
        updatedTariff.Name = updateDto.Name!;

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.UpdateTariffPlanAsync(tariffId, It.IsAny<DirectoryDto.UpdateTariffPlanDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTariff);

        // Act
        var result = await _sut.Update(tariffId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariff = okResult.Value.ShouldBeOfType<DirectoryDto.TariffPlanDto>();
        tariff.Id.ShouldBe(tariffId);
        tariff.Name.ShouldBe(updatedTariff.Name);

        _directoryClientMock.Verify(
            c => c.UpdateTariffPlanAsync(tariffId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenTariffNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.UpdateTariffPlanAsync(tariffId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Not found", null, System.Net.HttpStatusCode.NotFound));

        // Act
        var result = await _sut.Update(tariffId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(tariffId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _directoryClientMock.Verify(
            c => c.UpdateTariffPlanAsync(tariffId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenOtherExceptionThrown_ThrowsException()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();
        var errorMessage = "Validation error";

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.UpdateTariffPlanAsync(tariffId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            () => _sut.Update(tariffId, updateDto));

        exception.Message.ShouldBe(errorMessage);

        _directoryClientMock.Verify(
            c => c.UpdateTariffPlanAsync(tariffId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenTariffExists_ReturnsNoContent()
    {
        // Arrange
        var tariffId = Guid.NewGuid();

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.DeleteTariffPlanAsync(tariffId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(tariffId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _directoryClientMock.Verify(
            c => c.DeleteTariffPlanAsync(tariffId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenTariffNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var tariffId = Guid.NewGuid();

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.DeleteTariffPlanAsync(tariffId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Not found", null, System.Net.HttpStatusCode.NotFound));

        // Act
        var result = await _sut.Delete(tariffId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(tariffId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _directoryClientMock.Verify(
            c => c.DeleteTariffPlanAsync(tariffId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenOtherExceptionThrown_ThrowsException()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var errorMessage = "Database error";

        SetupUserWithRoles("ServiceManager");

        _directoryClientMock
            .Setup(c => c.DeleteTariffPlanAsync(tariffId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            () => _sut.Delete(tariffId));

        exception.Message.ShouldBe(errorMessage);

        _directoryClientMock.Verify(
            c => c.DeleteTariffPlanAsync(tariffId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupUserWithRoles(params string[] roles)
    {
        var claims = new List<Claim>();

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        // Настраиваем ControllerContext
        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ControllerActionDescriptor()
        };

        _sut.ControllerContext = controllerContext;
    }

    private static DirectoryDto.TariffPlanDto CreateTariffPlanDto(Guid? id = null)
    {
        var faker = new AutoFaker<DirectoryDto.TariffPlanDto>();

        if (id.HasValue)
            faker.RuleFor(dto => dto.Id, _ => id.Value);

        return faker
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, _ => Guid.NewGuid())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<DirectoryDto.TariffPlanDto> CreateTariffPlanDtoList(int count)
    {
        var list = new List<DirectoryDto.TariffPlanDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanDto());
        }
        return list;
    }

    private static DirectoryDto.TariffPlanShortDataDto CreateTariffPlanShortDataDto()
    {
        var faker = new AutoFaker<DirectoryDto.TariffPlanShortDataDto>();

        return faker
            .RuleFor(dto => dto.Id, f => f.Random.Guid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .Generate();
    }

    private static List<DirectoryDto.TariffPlanShortDataDto> CreateTariffPlanShortDataDtoList(int count)
    {
        var list = new List<DirectoryDto.TariffPlanShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanShortDataDto());
        }
        return list;
    }

    private static DirectoryDto.CreateTariffPlanDto CreateCreateTariffPlanDto()
    {
        return new AutoFaker<DirectoryDto.CreateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, _ => Guid.NewGuid())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static DirectoryDto.UpdateTariffPlanDto CreateUpdateTariffPlanDto()
    {
        return new AutoFaker<DirectoryDto.UpdateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    #endregion
}