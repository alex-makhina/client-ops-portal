using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Moq;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class ServiceServiceTests
{
    private readonly Mock<IGenericRepository<Service>> _serviceRepoMock;
    private readonly Mock<IGenericRepository<TariffPlan>> _tariffRepoMock;
    private readonly ServiceService _service;

    public ServiceServiceTests()
    {
        _serviceRepoMock = new Mock<IGenericRepository<Service>>();
        _tariffRepoMock = new Mock<IGenericRepository<TariffPlan>>();
        _service = new ServiceService(_serviceRepoMock.Object, _tariffRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_UniqueName_ReturnsCreatedDto()
    {
        // Arrange
        var createDto = new CreateServiceDto { Name = "Домашний Интернет", Description = "Описание" };
        _serviceRepoMock.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Service, bool>>>(), false, default))
            .ReturnsAsync(Array.Empty<Service>());
        _serviceRepoMock.Setup(r => r.AddAsync(It.IsAny<Service>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        _serviceRepoMock.Verify(r => r.AddAsync(It.IsAny<Service>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateServiceDto
        {
            Name = "Существующая Услуга",
            Description = "Тестовое описание"
        };

        _serviceRepoMock.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Service, bool>>>(), false, default))
            .ReturnsAsync(new[] 
            { 
                new Service 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Существующая Услуга",
                    Description = "Тестовое описание"
                } 
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
        Assert.Contains("уже существует", ex.Message);
        _serviceRepoMock.Verify(r => r.AddAsync(It.IsAny<Service>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_UpdatesServiceAndReturnsDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var existingService = new Service { Id = serviceId, Name = "Старое название", Description = "Старое" };
        var updateDto = new UpdateServiceDto { Name = "Новое название", Description = "Новое" };

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, false, default)).ReturnsAsync(existingService);
        _serviceRepoMock.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Service, bool>>>(), false, default))
            .ReturnsAsync(Array.Empty<Service>());
        _serviceRepoMock.Setup(r => r.UpdateAsync(existingService, default)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(serviceId, updateDto);

        // Assert
        Assert.Equal(updateDto.Name, result.Name);
        _serviceRepoMock.Verify(r => r.UpdateAsync(existingService, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ServiceNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        _serviceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), false, default)).ReturnsAsync((Service?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), 
            new UpdateServiceDto 
            { 
                Name = "Тестовое название",
                Description = "Тестовое описание"
            }));
    }

    [Fact]
    public async Task UpdateAsync_WithTariffPlans_SyncsAddUpdateDelete()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffToUpdateId = Guid.NewGuid();
        var tariffToDeleteId = Guid.NewGuid(); 
        var existingService = new Service { Id = serviceId, Name = "Test", Description = "Тестовое описание" };

        var existingTariffs = new[]
        {
        new TariffPlan { Id = tariffToUpdateId, ServiceId = serviceId, Name = "Старый", Price = 100, Description = "Тестовое описание" },
        new TariffPlan { Id = tariffToDeleteId, ServiceId = serviceId, Name = "На удаление", Price = 50, Description = "Тестовое описание" }
    };

        _tariffRepoMock.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<TariffPlan, bool>>>(), false, default))
            .ReturnsAsync(existingTariffs);

        var updateDto = new UpdateServiceDto
        {
            Name = "Test",
            Description = "Тестовое описание",
            TariffPlans = new List<UpdateTariffPlanFromServiceDto>
        {
            new() { Id = tariffToUpdateId, Name = "Обновленный", Price = 150, Description = "Обновленное описание" },
            new() { Id = Guid.Empty, Name = "Новый", Price = 200, Description = "Новое описание" }
        }
        };

        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, false, default)).ReturnsAsync(existingService);
        _serviceRepoMock.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Service, bool>>>(), false, default))
            .ReturnsAsync(Array.Empty<Service>());
        _serviceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Service>(), default)).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(serviceId, updateDto);

        // Assert
        _tariffRepoMock.Verify(r => r.DeleteAsync(tariffToDeleteId, default), Times.Once);
        _tariffRepoMock.Verify(r => r.UpdateAsync(It.Is<TariffPlan>(t => t.Id == tariffToUpdateId), default), Times.Once);
        _tariffRepoMock.Verify(r => r.AddAsync(It.Is<TariffPlan>(t => t.Name == "Новый"), default), Times.Once);
    }

    [Fact]
    public async Task GetActiveServicesAsync_ReturnsOnlyActiveServices()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var services = new[]
        {
            new Service { Id = Guid.NewGuid(), Name = "Active1", EndDate = null, Description = "Описание1" },
            new Service { Id = Guid.NewGuid(), Name = "Active2", EndDate = now.AddDays(10), Description = "Описание2" },
            new Service { Id = Guid.NewGuid(), Name = "Expired", EndDate = now.AddDays(-1), Description = "Описание3" }
        };

        _serviceRepoMock.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Service, bool>>>(), false, default))
            .ReturnsAsync(services.Where(s => s.EndDate == null || s.EndDate > now).ToList());

        // Act
        var result = await _service.GetActiveServicesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, s => s.Name == "Expired");
    }

    [Fact]
    public async Task GetFullServiceDataAsync_ValidId_ReturnsFullDataDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var service = new Service { Id = serviceId, Name = "FullTest", Description = "Тестовове описание" };
        _serviceRepoMock.Setup(r => r.GetByIdAsync(serviceId, true, default)).ReturnsAsync(service);

        // Act
        var result = await _service.GetFullServiceDataAsync(serviceId);

        // Assert
        Assert.NotNull(result);
        _serviceRepoMock.Verify(r => r.GetByIdAsync(serviceId, true, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _serviceRepoMock.Setup(r => r.DeleteAsync(serviceId, default)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(serviceId);

        // Assert
        _serviceRepoMock.Verify(r => r.DeleteAsync(serviceId, default), Times.Once);
    }
}