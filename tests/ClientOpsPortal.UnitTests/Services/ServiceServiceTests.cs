using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class ServiceServiceTests
{
    private readonly Mock<IGenericRepository<Service>> _serviceRepoMock;
    private readonly Mock<IGenericRepository<TariffPlan>> _tariffRepoMock;
    private readonly ServiceService _sut;

    public ServiceServiceTests()
    {
        _serviceRepoMock = new Mock<IGenericRepository<Service>>();
        _tariffRepoMock = new Mock<IGenericRepository<TariffPlan>>();
        _sut = new ServiceService(_serviceRepoMock.Object, _tariffRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsUnique_ReturnsCreatedDto()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();

        _serviceRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Service>());

        _serviceRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.Description.ShouldBe(createDto.Description);

        _serviceRepoMock.Verify(
            r => r.AddAsync(
                It.Is<Service>(s => s.Name == createDto.Name && s.Description == createDto.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var existingService = CreateService(createDto.Name, createDto.Description);

        _serviceRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingService });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateAsync(createDto));

        exception.Message.ShouldContain("уже существует");

        _serviceRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesServiceAndReturnsDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var existingService = CreateService("Старое название", "Старое описание", serviceId);
        var updateDto = CreateUpdateServiceDto("Новое название", "Новое описание");

        var existingTariffPlans = new List<TariffPlan>();

        _serviceRepoMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _serviceRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Service>());

        _tariffRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlans); 

        _serviceRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(serviceId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(updateDto.Name);
        result.Description.ShouldBe(updateDto.Description);

        _serviceRepoMock.Verify(
            r => r.UpdateAsync(
                It.Is<Service>(s => s.Name == updateDto.Name && s.Description == updateDto.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenServiceNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();

        _serviceRepoMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(serviceId, updateDto));

        _serviceRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenTariffPlansProvided_SyncsAddUpdateDelete()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffToUpdateId = Guid.NewGuid();
        var tariffToDeleteId = Guid.NewGuid();

        var existingService = CreateService("Test", "Test Description", serviceId);
        var existingTariffs = new[]
        {
            CreateTariffPlan(tariffToUpdateId, "Старый", 100, serviceId),
            CreateTariffPlan(tariffToDeleteId, "На удаление", 50, serviceId)
        };

        var updateDto = new UpdateServiceDto
        {
            Name = "Test",
            Description = "Test Description",
            TariffPlans = new List<UpdateTariffPlanFromServiceDto>
            {
                new()
                {
                    Id = tariffToUpdateId,
                    Name = "Обновленный",
                    Price = 150,
                    Description = "Обновленное описание"
                },
                new()
                {
                    Id = Guid.Empty,
                    Name = "Новый",
                    Price = 200,
                    Description = "Новое описание"
                }
            }
        };

        _serviceRepoMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _tariffRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffs);

        _serviceRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Service>());

        _serviceRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(serviceId, updateDto);

        // Assert
        _tariffRepoMock.Verify(
            r => r.DeleteAsync(tariffToDeleteId, It.IsAny<CancellationToken>()),
            Times.Once);

        _tariffRepoMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.Id == tariffToUpdateId && t.Name == "Обновленный" && t.Price == 150),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _tariffRepoMock.Verify(
            r => r.AddAsync(
                It.Is<TariffPlan>(t => t.Name == "Новый" && t.Price == 200 && t.ServiceId == serviceId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveServicesAsync_ReturnsOnlyActiveServices()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var activeService1 = CreateService("Active1", "Description1", endDate: null);
        var activeService2 = CreateService("Active2", "Description2", endDate: now.AddDays(10));
        var expiredService = CreateService("Expired", "Description3", endDate: now.AddDays(-1));

        var allServices = new[] { activeService1, activeService2, expiredService };
        var activeServices = allServices
            .Where(s => s.EndDate == null || s.EndDate > now)
            .ToList();

        _serviceRepoMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeServices);

        // Act
        var result = await _sut.GetActiveServicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldNotContain(s => s.Name == "Expired");
        result.ShouldContain(s => s.Name == "Active1");
        result.ShouldContain(s => s.Name == "Active2");
    }

    [Fact]
    public async Task GetFullServiceDataAsync_WhenValidId_ReturnsFullDataDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var service = CreateService("FullTest", "Test Description", serviceId);

        _serviceRepoMock
            .Setup(r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetFullServiceDataAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
        result.Name.ShouldBe(service.Name);

        _serviceRepoMock.Verify(
            r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenValidId_CallsRepositoryDelete()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceRepoMock
            .Setup(r => r.DeleteAsync(serviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(serviceId);

        // Assert
        _serviceRepoMock.Verify(
            r => r.DeleteAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region Helper Methods

    private static CreateServiceDto CreateCreateServiceDto() =>
        new AutoFaker<CreateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .Generate();

    private static UpdateServiceDto CreateUpdateServiceDto(string? name = null, string? description = null) =>
        new AutoFaker<UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => name ?? f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => description ?? f.Lorem.Sentence())
            .RuleFor(dto => dto.TariffPlans, new List<UpdateTariffPlanFromServiceDto>())
            .Generate();

    private static Service CreateService(string name, string description, Guid? id = null, DateTimeOffset? endDate = null) =>
        new AutoFaker<Service>()
            .RuleFor(s => s.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(s => s.Name, _ => name)
            .RuleFor(s => s.Description, _ => description)
            .RuleFor(s => s.EndDate, _ => endDate)
            .RuleFor(s => s.TariffPlans, new List<TariffPlan>())
            .Generate();

    private static TariffPlan CreateTariffPlan(Guid id, string name, decimal price, Guid serviceId) =>
        new AutoFaker<TariffPlan>()
            .RuleFor(t => t.Id, _ => id)
            .RuleFor(t => t.Name, _ => name)
            .RuleFor(t => t.Price, _ => price)
            .RuleFor(t => t.ServiceId, _ => serviceId)
            .RuleFor(t => t.Description, f => f.Lorem.Sentence())
            .Generate();

    #endregion
}