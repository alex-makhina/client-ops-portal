using AutoBogus;
using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;

namespace ClientOpsPortal.UnitTests.Controllers;

public class TariffPlansControllerTests
{
    private readonly Mock<ITariffPlanService> _tariffPlanServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TariffPlansController _sut;

    public TariffPlansControllerTests()
    {
        _tariffPlanServiceMock = new Mock<ITariffPlanService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new TariffPlansController(
            _tariffPlanServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenTariffPlansExist_ReturnsOkWithTariffPlans()
    {
        // Arrange
        var expectedTariffPlans = CreateTariffPlanDtoList(5);

        _tariffPlanServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffPlans);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlans = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffPlans.Count().ShouldBe(5);

        _tariffPlanServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoTariffPlansExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<TariffPlanDto>().AsReadOnly();

        _tariffPlanServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlans = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffPlans.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedTariffPlans = CreateTariffPlanDtoList(3);

        _tariffPlanServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffPlans);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _tariffPlanServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenTariffPlanExists_ReturnsOkWithTariffPlan()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var expectedTariffPlan = CreateTariffPlanDto(tariffPlanId);

        _tariffPlanServiceMock
            .Setup(s => s.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffPlan);

        // Act
        var result = await _sut.GetById(tariffPlanId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlan = okResult.Value.ShouldBeOfType<TariffPlanDto>();

        tariffPlan.Id.ShouldBe(tariffPlanId);

        _tariffPlanServiceMock.Verify(
            s => s.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenTariffPlanNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();

        _tariffPlanServiceMock
            .Setup(s => s.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TariffPlanDto?)null);

        // Act
        var result = await _sut.GetById(tariffPlanId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(tariffPlanId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _tariffPlanServiceMock.Verify(
            s => s.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToService()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var expectedTariffPlan = CreateTariffPlanDto(tariffPlanId);

        _tariffPlanServiceMock
            .Setup(s => s.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffPlan);

        // Act
        var result = await _sut.GetById(tariffPlanId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _tariffPlanServiceMock.Verify(
            s => s.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByService Tests

    [Fact]
    public async Task GetByService_WhenTariffPlansExist_ReturnsOkWithTariffPlans()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedTariffPlans = CreateTariffPlanDtoList(4);

        _tariffPlanServiceMock
            .Setup(s => s.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffPlans);

        // Act
        var result = await _sut.GetByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlans = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffPlans.Count().ShouldBe(4);

        _tariffPlanServiceMock.Verify(
            s => s.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByService_WhenNoTariffPlansExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var emptyList = new List<TariffPlanDto>().AsReadOnly();

        _tariffPlanServiceMock
            .Setup(s => s.GetTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlans = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffPlans.ShouldBeEmpty();
    }

    #endregion

    #region GetActiveByService Tests

    [Fact]
    public async Task GetActiveByService_WhenActiveTariffPlansExist_ReturnsOkWithShortDataDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedTariffPlans = CreateTariffPlanShortDataDtoList(3);

        _tariffPlanServiceMock
            .Setup(s => s.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTariffPlans);

        // Act
        var result = await _sut.GetActiveByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlans = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanShortDataDto>>();
        tariffPlans.Count().ShouldBe(3);

        _tariffPlanServiceMock.Verify(
            s => s.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveByService_WhenNoActiveTariffPlansExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var emptyList = new List<TariffPlanShortDataDto>().AsReadOnly();

        _tariffPlanServiceMock
            .Setup(s => s.GetActiveTariffPlansByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetActiveByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlans = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanShortDataDto>>();
        tariffPlans.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithTariffPlan()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdDto = CreateTariffPlanDto(id: Guid.NewGuid(), serviceId: createDto.ServiceId);

        SetupUserRole("ServiceManager");

        _tariffPlanServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(TariffPlansController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<TariffPlanDto>().ShouldBe(createdDto);

        _tariffPlanServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var errorMessage = "Ошибка создания тарифного плана";

        SetupUserRole("ServiceManager");

        _tariffPlanServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _tariffPlanServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenTariffPlanExists_ReturnsOkWithUpdatedTariffPlan()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();
        var updatedDto = CreateTariffPlanDto(tariffPlanId);

        SetupUserRole("ServiceManager");

        _tariffPlanServiceMock
            .Setup(s => s.UpdateAsync(tariffPlanId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(tariffPlanId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffPlan = okResult.Value.ShouldBeOfType<TariffPlanDto>();

        tariffPlan.Id.ShouldBe(tariffPlanId);

        _tariffPlanServiceMock.Verify(
            s => s.UpdateAsync(tariffPlanId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenTariffPlanNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();

        SetupUserRole("ServiceManager");

        _tariffPlanServiceMock
            .Setup(s => s.UpdateAsync(tariffPlanId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(TariffPlan), tariffPlanId));

        // Act
        var result = await _sut.Update(tariffPlanId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(tariffPlanId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenTariffPlanExists_ReturnsNoContent()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();

        SetupUserRole("ServiceManager");

        _tariffPlanServiceMock
            .Setup(s => s.DeleteAsync(tariffPlanId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(tariffPlanId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _tariffPlanServiceMock.Verify(
            s => s.DeleteAsync(tariffPlanId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenTariffPlanNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();

        SetupUserRole("ServiceManager");

        _tariffPlanServiceMock
            .Setup(s => s.DeleteAsync(tariffPlanId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(TariffPlan), tariffPlanId));

        // Act
        var result = await _sut.Delete(tariffPlanId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(tariffPlanId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Helper Methods

    private void SetupUserRole(string role, Guid? userId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role)
        };

        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(userId);
    }

    private static CreateTariffPlanDto CreateCreateTariffPlanDto()
    {
        return new AutoFaker<CreateTariffPlanDto>().Generate();
    }

    private static UpdateTariffPlanDto CreateUpdateTariffPlanDto()
    {
        return new AutoFaker<UpdateTariffPlanDto>().Generate();
    }

    private static TariffPlanDto CreateTariffPlanDto(Guid? id = null, Guid? serviceId = null)
    {
        var autoFaker = new AutoFaker<TariffPlanDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);

        if (serviceId.HasValue)
            autoFaker.RuleFor(dto => dto.ServiceId, _ => serviceId.Value);

        return autoFaker.Generate();
    }

    private static IReadOnlyCollection<TariffPlanDto> CreateTariffPlanDtoList(int count)
    {
        var list = new List<TariffPlanDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanDto());
        }
        return list.AsReadOnly();
    }

    private static TariffPlanShortDataDto CreateTariffPlanShortDataDto()
    {
        return new AutoFaker<TariffPlanShortDataDto>().Generate();
    }

    private static IReadOnlyCollection<TariffPlanShortDataDto> CreateTariffPlanShortDataDtoList(int count)
    {
        var list = new List<TariffPlanShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanShortDataDto());
        }
        return list.AsReadOnly();
    }

    #endregion
}