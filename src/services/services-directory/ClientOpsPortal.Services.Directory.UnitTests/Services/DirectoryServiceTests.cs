using AutoBogus;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Contracts.Exceptions;
using ClientOpsPortal.Services.Directory.Data;
using ClientOpsPortal.Services.Directory.Data.Entities;
using ClientOpsPortal.Services.Directory.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using System.Text.Json;
using Xunit;

namespace ClientOpsPortal.Services.Directory.UnitTests.Services;

public class DirectoryServiceTests : IDisposable
{
    private readonly DirectoryDbContext _context;
    private readonly ServiceRepository _serviceRepo;
    private readonly GenericRepository<TariffPlan> _tariffRepo;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly DirectoryService _service;

    public DirectoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<DirectoryDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new DirectoryDbContext(options);
        _serviceRepo = new ServiceRepository(_context);
        _tariffRepo = new GenericRepository<TariffPlan>(_context);
        _cacheMock = new Mock<IDistributedCache>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        // Настройка моков для кеша
        _cacheMock.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _cacheMock.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock.Setup(x => x.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheOptions = Options.Create(new ServiceCacheOptions
        {
            ActiveServicesMinutes = 120,
            ServiceByIdMinutes = 120
        });

        _service = new DirectoryService(
            _serviceRepo,
            _tariffRepo,
            _cacheMock.Object,
            cacheOptions,
            _publishEndpointMock.Object,
            NullLogger<DirectoryService>.Instance);
    }

    #region Services Tests

    [Fact]
    public async Task GetAllServicesAsync_WhenServicesExist_ReturnsServiceDtos()
    {
        // Arrange
        var services = CreateServiceEntityList(3);
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllServicesAsync(false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetAllServicesAsync_WhenNoServicesExist_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllServicesAsync(false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenCached_ReturnsCachedValue()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedDto = CreateServiceDto(serviceId);
        var json = JsonSerializer.Serialize(expectedDto);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(
                It.Is<string>(s => s == $"directory:service:{serviceId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.GetServiceByIdAsync(serviceId, false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenNotCached_ReturnsFromRepository()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var service = CreateServiceEntity(serviceId);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetServiceByIdAsync(serviceId, false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
        result.Name.ShouldBe(service.Name);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Act
        var result = await _service.GetServiceByIdAsync(Guid.NewGuid(), false, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveServicesAsync_WhenCached_ReturnsCachedValue()
    {
        // Arrange
        var expectedServices = CreateServiceShortDataDtoList(2);
        var json = JsonSerializer.Serialize(expectedServices);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(
                It.Is<string>(s => s == "directory:services:active"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.GetActiveServicesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CreateServiceAsync_WhenNameUnique_CreatesService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();

        // Act
        var result = await _service.CreateServiceAsync(createDto, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);

        var savedService = await _context.Services.FirstOrDefaultAsync(s => s.Name == createDto.Name);
        savedService.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateServiceAsync_WhenNameNotUnique_ThrowsException()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();

        // Создаем первую услугу
        await _service.CreateServiceAsync(createDto, CancellationToken.None);

        var duplicateDto = CreateCreateServiceDto();
        duplicateDto.Name = createDto.Name;

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _service.CreateServiceAsync(duplicateDto, CancellationToken.None));
        exception.Message.ShouldContain(createDto.Name);
    }

    [Fact]
    public async Task UpdateServiceAsync_WhenServiceExists_UpdatesService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdService = await _service.CreateServiceAsync(createDto, CancellationToken.None);
        var updateDto = CreateUpdateServiceDto();

        // Act
        var result = await _service.UpdateServiceAsync(createdService.Id, updateDto, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdService.Id);
        result.Name.ShouldBe(updateDto.Name);

        // Проверяем в базе
        var updatedService = await _context.Services.FindAsync(createdService.Id);
        updatedService.ShouldNotBeNull();
        updatedService.Name.ShouldBe(updateDto.Name);
    }

    [Fact]
    public async Task UpdateServiceAsync_WhenServiceNotFound_ThrowsException()
    {
        // Arrange
        var updateDto = CreateUpdateServiceDto();

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _service.UpdateServiceAsync(Guid.NewGuid(), updateDto, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteServiceAsync_WhenServiceExists_DeletesService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdService = await _service.CreateServiceAsync(createDto, CancellationToken.None);

        // Act
        await _service.DeleteServiceAsync(createdService.Id, CancellationToken.None);

        // Assert
        var deletedService = await _context.Services.FindAsync(createdService.Id);
        deletedService.ShouldBeNull();
    }

    #endregion

    #region TariffPlans Tests

    [Fact]
    public async Task GetAllTariffPlansAsync_WhenTariffsExist_ReturnsTariffDtos()
    {
        // Arrange
        var tariffs = CreateTariffPlanEntityList(3);
        await _context.TariffPlans.AddRangeAsync(tariffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTariffPlansAsync(false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetTariffPlanByIdAsync_WhenCached_ReturnsCachedValue()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var expectedDto = CreateTariffPlanDto(tariffId);
        var json = JsonSerializer.Serialize(expectedDto);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(
                It.Is<string>(s => s == $"directory:tariff:{tariffId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.GetTariffPlanByIdAsync(tariffId, false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tariffId);
    }

    [Fact]
    public async Task GetTariffPlanByIdAsync_WhenNotCached_ReturnsFromRepository()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var tariff = CreateTariffPlanEntity(tariffId);
        await _context.TariffPlans.AddAsync(tariff);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTariffPlanByIdAsync(tariffId, false, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tariffId);
        result.Name.ShouldBe(tariff.Name);
    }

    [Fact]
    public async Task GetTariffPlansByServiceAsync_WhenTariffsExist_ReturnsTariffDtos()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffs = CreateTariffPlanEntityList(3, serviceId);
        await _context.TariffPlans.AddRangeAsync(tariffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTariffPlansByServiceAsync(serviceId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_WhenCached_ReturnsCachedValue()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var expectedTariffs = CreateTariffPlanShortDataDtoList(2);
        var json = JsonSerializer.Serialize(expectedTariffs);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _cacheMock.Setup(x => x.GetAsync(
                It.Is<string>(s => s == $"directory:tariffs:active:{serviceId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.GetActiveTariffPlansByServiceAsync(serviceId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CreateTariffPlanAsync_ValidDto_CreatesTariffPlan()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();

        // Act
        var result = await _service.CreateTariffPlanAsync(createDto, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.ServiceId.ShouldBe(createDto.ServiceId);

        // Проверяем в базе
        var savedTariff = await _context.TariffPlans.FirstOrDefaultAsync(t => t.Name == createDto.Name);
        savedTariff.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateTariffPlanAsync_WhenTariffExists_UpdatesTariffPlan()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdTariff = await _service.CreateTariffPlanAsync(createDto, CancellationToken.None);
        var updateDto = CreateUpdateTariffPlanDto();

        // Act
        var result = await _service.UpdateTariffPlanAsync(createdTariff.Id, updateDto, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdTariff.Id);

        // Проверяем в базе
        var updatedTariff = await _context.TariffPlans.FindAsync(createdTariff.Id);
        updatedTariff.ShouldNotBeNull();
        updatedTariff.Name.ShouldBe(updateDto.Name);
    }

    [Fact]
    public async Task UpdateTariffPlanAsync_WhenTariffNotFound_ThrowsException()
    {
        // Arrange
        var updateDto = CreateUpdateTariffPlanDto();

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _service.UpdateTariffPlanAsync(Guid.NewGuid(), updateDto, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteTariffPlanAsync_WhenTariffExists_DeletesTariffPlan()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdTariff = await _service.CreateTariffPlanAsync(createDto, CancellationToken.None);

        // Act
        await _service.DeleteTariffPlanAsync(createdTariff.Id, CancellationToken.None);

        // Assert
        var deletedTariff = await _context.TariffPlans.FindAsync(createdTariff.Id);
        deletedTariff.ShouldBeNull();
    }

    #endregion

    #region Helper Methods

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

    private static TariffPlan CreateTariffPlanEntity(Guid? id = null, Guid? serviceId = null)
    {
        var tariffId = id ?? Guid.NewGuid();
        return new TariffPlan
        {
            Id = tariffId,
            Name = $"Tariff {tariffId:N}",
            Description = $"Description for tariff {tariffId:N}",
            Price = new Random().Next(1, 999),
            ServiceId = serviceId ?? Guid.NewGuid(),
            BeginDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = null
        };
    }

    private static List<TariffPlan> CreateTariffPlanEntityList(int count, Guid? serviceId = null)
    {
        var list = new List<TariffPlan>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanEntity(serviceId: serviceId));
        }
        return list;
    }

    private static ServiceDto CreateServiceDto(Guid? id = null)
    {
        return new AutoFaker<ServiceDto>()
            .RuleFor(dto => dto.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<ServiceShortDataDto> CreateServiceShortDataDtoList(int count)
    {
        var list = new List<ServiceShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new AutoFaker<ServiceShortDataDto>()
                .RuleFor(dto => dto.Id, f => f.Random.Guid())
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
                .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
                .RuleFor(dto => dto.EndDate, _ => null)
                .Generate());
        }
        return list;
    }

    private static TariffPlanDto CreateTariffPlanDto(Guid? id = null)
    {
        return new AutoFaker<TariffPlanDto>()
            .RuleFor(dto => dto.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, f => f.Random.Guid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<TariffPlanShortDataDto> CreateTariffPlanShortDataDtoList(int count)
    {
        var list = new List<TariffPlanShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new AutoFaker<TariffPlanShortDataDto>()
                .RuleFor(dto => dto.Id, f => f.Random.Guid())
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
                .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
                .Generate());
        }
        return list;
    }

    private static CreateServiceDto CreateCreateServiceDto()
    {
        return new AutoFaker<CreateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<CreateTariffPlanDto>())
            .Generate();
    }

    private static UpdateServiceDto CreateUpdateServiceDto()
    {
        return new AutoFaker<UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<UpdateTariffPlanFromServiceDto>())
            .Generate();
    }

    private static CreateTariffPlanDto CreateCreateTariffPlanDto()
    {
        return new AutoFaker<CreateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, f => f.Random.Guid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static UpdateTariffPlanDto CreateUpdateTariffPlanDto()
    {
        return new AutoFaker<UpdateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}