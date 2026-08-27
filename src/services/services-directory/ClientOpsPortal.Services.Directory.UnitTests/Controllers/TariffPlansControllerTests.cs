using AutoBogus;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Contracts.Exceptions;
using ClientOpsPortal.Services.Directory.Controllers;
using ClientOpsPortal.Services.Directory.Data;
using ClientOpsPortal.Services.Directory.Data.Entities;
using ClientOpsPortal.Services.Directory.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace ClientOpsPortal.Services.Directory.UnitTests.Controllers;

public class TariffPlansControllerTests : IDisposable
{
    private readonly DirectoryDbContext _context;
    private readonly ServiceRepository _serviceRepo;
    private readonly GenericRepository<TariffPlan> _tariffRepo;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly DirectoryService _service;
    private readonly TariffPlansController _controller;

    public TariffPlansControllerTests()
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

        _controller = new TariffPlansController(_service);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenTariffPlansExist_ReturnsOkWithTariffPlans()
    {
        // Arrange
        var tariffs = CreateTariffPlanEntityList(3);
        await _context.TariffPlans.AddRangeAsync(tariffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffDtos.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetAll_WhenNoTariffPlansExist_ReturnsOkWithEmptyList()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffDtos.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_ReturnsTariffsWithServices()
    {
        // Arrange
        var service = CreateServiceEntity(Guid.NewGuid());
        var tariffs = CreateTariffPlanEntityList(2, service.Id);
        await _context.Services.AddAsync(service);
        await _context.TariffPlans.AddRangeAsync(tariffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll(true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffDtos.Count().ShouldBe(2);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenTariffPlanExists_ReturnsOkWithTariffPlan()
    {
        // Arrange
        var tariff = CreateTariffPlanEntity(Guid.NewGuid());
        await _context.TariffPlans.AddAsync(tariff);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(tariff.Id);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffDto = okResult.Value.ShouldBeOfType<TariffPlanDto>();
        tariffDto.Id.ShouldBe(tariff.Id);
        tariffDto.Name.ShouldBe(tariff.Name);
    }

    [Fact]
    public async Task GetById_WhenTariffPlanNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetById(Guid.NewGuid());

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region GetByService Tests

    [Fact]
    public async Task GetByService_WhenTariffPlansExist_ReturnsOk()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffs = CreateTariffPlanEntityList(3, serviceId);
        await _context.TariffPlans.AddRangeAsync(tariffs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffDtos.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetByService_WhenNoTariffPlansExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _controller.GetByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var tariffDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanDto>>();
        tariffDtos.ShouldBeEmpty();
    }

    #endregion

    #region GetActiveByService Tests

    [Fact]
    public async Task GetActiveByService_WhenActiveTariffPlansExist_ReturnsOkWithShortDtos()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var activeTariff = CreateTariffPlanEntity(serviceId: serviceId, endDate: null);
        var inactiveTariff = CreateTariffPlanEntity(serviceId: serviceId, endDate: now.AddDays(-1));

        await _context.TariffPlans.AddRangeAsync(activeTariff, inactiveTariff);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetActiveByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var shortTariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanShortDataDto>>();
        shortTariffs.Count().ShouldBe(1); // Только активный тариф
    }

    [Fact]
    public async Task GetActiveByService_WhenNoActiveTariffPlansExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var inactiveTariff = CreateTariffPlanEntity(serviceId: serviceId, endDate: now.AddDays(-1));

        await _context.TariffPlans.AddAsync(inactiveTariff);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetActiveByService(serviceId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var shortTariffs = okResult.Value.ShouldBeAssignableTo<IEnumerable<TariffPlanShortDataDto>>();
        shortTariffs.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(TariffPlansController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldNotBeNull();
        createdResult.Value.ShouldBeOfType<TariffPlanDto>();

        var tariffId = (Guid)createdResult.RouteValues["id"]!;
        var savedTariff = await _context.TariffPlans.FindAsync(tariffId);
        savedTariff.ShouldNotBeNull();
        savedTariff.Name.ShouldBe(createDto.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenTariffPlanExists_ReturnsOkWithUpdatedTariffPlan()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdResult = await _controller.Create(createDto);
        var createdDto = ((CreatedAtActionResult)createdResult).Value.ShouldBeOfType<TariffPlanDto>();

        var updateDto = CreateUpdateTariffPlanDto();

        // Act
        var result = await _controller.Update(createdDto.Id, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var updatedTariff = okResult.Value.ShouldBeOfType<TariffPlanDto>();
        updatedTariff.Id.ShouldBe(createdDto.Id);
        updatedTariff.Name.ShouldBe(updateDto.Name);

        // Проверяем в базе
        var tariffInDb = await _context.TariffPlans.FindAsync(createdDto.Id);
        tariffInDb.ShouldNotBeNull();
        tariffInDb.Name.ShouldBe(updateDto.Name);
    }

    [Fact]
    public async Task Update_WhenTariffPlanNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateDto = CreateUpdateTariffPlanDto();

        // Act
        var result = await _controller.Update(Guid.NewGuid(), updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenTariffPlanExists_ReturnsNoContent()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdResult = await _controller.Create(createDto);
        var createdDto = ((CreatedAtActionResult)createdResult).Value.ShouldBeOfType<TariffPlanDto>();

        // Act
        var result = await _controller.Delete(createdDto.Id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        // Проверяем, что тариф удален
        var deletedTariff = await _context.TariffPlans.FindAsync(createdDto.Id);
        deletedTariff.ShouldBeNull();
    }

    [Fact]
    public async Task Delete_WhenTariffPlanNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Delete(Guid.NewGuid());

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Helper Methods

    private static Service CreateServiceEntity(Guid? id = null)
    {
        var serviceId = id ?? Guid.NewGuid();
        return new Service
        {
            Id = serviceId,
            Name = $"Service {serviceId:N}",
            Description = $"Description for service {serviceId:N}",
            BeginDate = DateTimeOffset.UtcNow.AddDays(-10),
            EndDate = null,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user",
            TariffPlans = new List<TariffPlan>()
        };
    }

    private static TariffPlan CreateTariffPlanEntity(Guid? id = null, Guid? serviceId = null, DateTimeOffset? endDate = null)
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
            EndDate = endDate
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