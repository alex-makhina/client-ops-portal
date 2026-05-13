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

namespace ClientOpsPortal.UnitTests.Controllers;

public class ServicesControllerTests
{
    private readonly Mock<IServiceService> _serviceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ServicesController _sut;

    public ServicesControllerTests()
    {
        _serviceMock = new Mock<IServiceService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new ServicesController(_serviceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenServicesExist_ReturnsOkWithServices()
    {
        // Arrange
        var expectedServices = CreateServiceDtoList(3);

        _serviceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceDto>>();
        services.Count().ShouldBe(3);

        _serviceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoServicesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceDto>());

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceDto>>();
        services.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetById_WhenServiceExists_ReturnsOkWithService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedService = CreateServiceDto(serviceId);

        _serviceMock
            .Setup(s => s.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedService);

        // Act
        var result = await _sut.GetById(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var service = okResult.Value.ShouldBeOfType<ServiceDto>();

        service.Id.ShouldBe(serviceId);
        service.Name.ShouldBe(expectedService.Name);

        _serviceMock.Verify(
            s => s.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceDto?)null);

        // Act
        var result = await _sut.GetById(serviceId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _serviceMock.Verify(
            s => s.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveServices_WhenActiveServicesExist_ReturnsOkWithShortDtos()
    {
        // Arrange
        var expectedServices = CreateServiceShortDataDtoList(5);

        _serviceMock
            .Setup(s => s.GetActiveServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetActiveServices();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceShortDataDto>>();
        services.Count().ShouldBe(5);

        _serviceMock.Verify(
            s => s.GetActiveServicesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveServices_WhenNoActiveServices_ReturnsOkWithEmptyList()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetActiveServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceShortDataDto>());

        // Act
        var result = await _sut.GetActiveServices();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var services = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceShortDataDto>>();
        services.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdDto = CreateServiceDto(id: Guid.NewGuid(), name: createDto.Name);

        _serviceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(ServicesController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<ServiceDto>().ShouldBe(createdDto);

        _serviceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenDuplicateName_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var duplicateMessage = $"Услуга с названием '{createDto.Name}' уже существует";

        _serviceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(duplicateMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain("уже существует");

        _serviceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenServiceExists_ReturnsOkWithUpdatedService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();
        var updatedDto = CreateServiceDto(serviceId, updateDto.Name);

        _serviceMock
            .Setup(s => s.UpdateAsync(serviceId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(serviceId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var service = okResult.Value.ShouldBeOfType<ServiceDto>();

        service.Id.ShouldBe(serviceId);
        service.Name.ShouldBe(updateDto.Name);

        _serviceMock.Verify(
            s => s.UpdateAsync(serviceId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenServiceNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();

        _serviceMock
            .Setup(s => s.UpdateAsync(serviceId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Service), serviceId));

        // Act
        var result = await _sut.Update(serviceId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _serviceMock.Verify(
            s => s.UpdateAsync(serviceId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenServiceExists_ReturnsNoContent()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.DeleteAsync(serviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(serviceId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _serviceMock.Verify(
            s => s.DeleteAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenServiceNotFound_ReturnsNotFound()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.DeleteAsync(serviceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Service), serviceId));

        // Act
        var result = await _sut.Delete(serviceId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _serviceMock.Verify(
            s => s.DeleteAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region Helper Methods

    private static CreateServiceDto CreateCreateServiceDto() =>
        new AutoFaker<CreateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .Generate();

    private static UpdateServiceDto CreateUpdateServiceDto() =>
        new AutoFaker<UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.TariffPlans, new List<UpdateTariffPlanFromServiceDto>())
            .Generate();

    private static ServiceDto CreateServiceDto(Guid? id = null, string? name = null) =>
    new AutoFaker<ServiceDto>()
        .RuleFor(dto => dto.Id, _ => id ?? Guid.NewGuid())
        .RuleFor(dto => dto.Name, f => name ?? f.Commerce.ProductName())
        .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
        .RuleFor(dto => dto.CreatedAt, f => f.Date.PastOffset())
        .RuleFor(dto => dto.UpdatedAt, f => f.Date.RecentOffset())
        .Generate();

    private static List<ServiceDto> CreateServiceDtoList(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => CreateServiceDto())
            .ToList();

    private static ServiceShortDataDto CreateServiceShortDataDto() =>
        new AutoFaker<ServiceShortDataDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .Generate();

    private static List<ServiceShortDataDto> CreateServiceShortDataDtoList(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => CreateServiceShortDataDto())
            .ToList();

    #endregion
}