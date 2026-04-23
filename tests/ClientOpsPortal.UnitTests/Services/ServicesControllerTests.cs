using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ClientOpsPortal.UnitTests.Services;

public class ServicesControllerTests
{
    private readonly Mock<IServiceService> _serviceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ServicesController _controller;

    public ServicesControllerTests()
    {
        _serviceMock = new Mock<IServiceService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new ServicesController(_serviceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithServices()
    {
        // Arrange
        var expected = new[] { new ServiceDto { Id = Guid.NewGuid(), Name = "ТВ", Description = "Тестовое описание" } };
        _serviceMock.Setup(s => s.GetAllAsync(false, default)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var services = Assert.IsAssignableFrom<IEnumerable<ServiceDto>>(okResult.Value);
        Assert.Single(services);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new ServiceDto { Id = id, Name = "Интернет", Description = "Тестовое описание" };
        _serviceMock.Setup(s => s.GetByIdAsync(id, true, default)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), true, default)).ReturnsAsync((ServiceDto?)null);

        // Act
        var result = await _controller.GetById(Guid.NewGuid());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("не найдена", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetActiveServices_ReturnsOkWithShortDtos()
    {
        // Arrange
        var expected = new[] { new ServiceShortDataDto { Name = "Active", Description = "Тестовое описание" } };
        _serviceMock.Setup(s => s.GetActiveServicesAsync(default)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetActiveServices();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ServiceShortDataDto>>(okResult.Value);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateServiceDto { Name = "Новая услуга", Description = "Тестовое описание" };
        var createdDto = new ServiceDto { Id = Guid.NewGuid(), Name = createDto.Name, Description = "Тестовое описание" };
        _serviceMock.Setup(s => s.CreateAsync(createDto, default)).ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ServicesController.GetById), createdResult.ActionName);
        Assert.Equal(createdDto.Id, createdResult.RouteValues?["id"]);
        Assert.Equal(createdDto, createdResult.Value);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateServiceDto { Name = "Дубль", Description = "Тестовое описание" };
        _serviceMock.Setup(s => s.CreateAsync(createDto, default))
            .ThrowsAsync(new InvalidOperationException("Услуга с названием 'Дубль' уже существует"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("уже существует", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateServiceDto { Name = "Обновлено", Description = "Тестовое описание" };
        var updatedDto = new ServiceDto { Id = id, Name = updateDto.Name, Description = "Тестовое описание" };
        _serviceMock.Setup(s => s.UpdateAsync(id, updateDto, default)).ReturnsAsync(updatedDto);

        // Act
        var result = await _controller.Update(id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedDto, okResult.Value);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateServiceDto>(), default))
            .ThrowsAsync(new EntityNotFoundException(typeof(Service), id));

        // Act
        var result = await _controller.Update(id, new UpdateServiceDto { Name = "Test", Description = "Тестовое описание" });

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("не найдена", notFound.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}