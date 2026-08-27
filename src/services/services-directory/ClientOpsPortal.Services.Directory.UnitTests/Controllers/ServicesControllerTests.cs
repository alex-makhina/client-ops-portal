using AutoBogus;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
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

public class ServicesControllerTests : IDisposable
{
    private readonly DirectoryDbContext _context;
    private readonly ServiceRepository _serviceRepo;
    private readonly GenericRepository<TariffPlan> _tariffRepo;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly DirectoryService _service;
    private readonly ServicesController _controller;

    public ServicesControllerTests()
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

        _controller = new ServicesController(_service);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenServicesExist_ReturnsOkWithServices()
    {
        // Arrange
        var services = CreateServiceEntityList(3);
        await _context.Services.AddRangeAsync(services);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var serviceDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceDto>>();
        serviceDtos.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetAll_WhenNoServicesExist_ReturnsOkWithEmptyList()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var serviceDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceDto>>();
        serviceDtos.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_ReturnsServicesWithTariffs()
    {
        // Arrange
        var service = CreateServiceEntity(Guid.NewGuid());
        service.TariffPlans = CreateTariffPlanEntityList(2, service.Id);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll(true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var serviceDtos = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceDto>>();
        serviceDtos.Count().ShouldBe(1);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenServiceExists_ReturnsOkWithService()
    {
        // Arrange
        var service = CreateServiceEntity(Guid.NewGuid());
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(service.Id);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var serviceDto = okResult.Value.ShouldBeOfType<ServiceDto>();
        serviceDto.Id.ShouldBe(service.Id);
        serviceDto.Name.ShouldBe(service.Name);
    }

    [Fact]
    public async Task GetById_WhenServiceNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetById(Guid.NewGuid());

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region GetFullServiceData Tests

    [Fact]
    public async Task GetFullServiceData_WhenServiceExists_ReturnsOkWithFullData()
    {
        // Arrange
        var service = CreateServiceEntity(Guid.NewGuid());
        service.TariffPlans = CreateTariffPlanEntityList(2, service.Id);
        await _context.Services.AddAsync(service);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetFullServiceData(service.Id);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var fullData = okResult.Value.ShouldBeOfType<ServiceFullDataDto>();
        fullData.Id.ShouldBe(service.Id);
        fullData.TariffPlans.ShouldNotBeNull();
        fullData.TariffPlans.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetFullServiceData_WhenServiceNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetFullServiceData(Guid.NewGuid());

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region GetActiveServices Tests

    [Fact]
    public async Task GetActiveServices_WhenActiveServicesExist_ReturnsOkWithShortDtos()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var activeService1 = CreateServiceEntity(Guid.NewGuid(), endDate: null);
        var activeService2 = CreateServiceEntity(Guid.NewGuid(), endDate: null); // Обе активны
        var inactiveService = CreateServiceEntity(Guid.NewGuid(), endDate: now.AddDays(-1));

        await _context.Services.AddRangeAsync(activeService1, activeService2, inactiveService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetActiveServices();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var shortServices = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceShortDataDto>>();
        shortServices.Count().ShouldBe(2); // Только активные услуги
    }

    [Fact]
    public async Task GetActiveServices_WhenNoActiveServices_ReturnsOkWithEmptyList()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var inactiveService1 = CreateServiceEntity(Guid.NewGuid(), endDate: now.AddDays(-1));
        var inactiveService2 = CreateServiceEntity(Guid.NewGuid(), endDate: now.AddDays(-2));

        await _context.Services.AddRangeAsync(inactiveService1, inactiveService2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetActiveServices();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var shortServices = okResult.Value.ShouldBeAssignableTo<IEnumerable<ServiceShortDataDto>>();
        shortServices.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(ServicesController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldNotBeNull();
        createdResult.Value.ShouldBeOfType<ServiceDto>();

        var serviceId = (Guid)createdResult.RouteValues["id"]!;
        var savedService = await _context.Services.FindAsync(serviceId);
        savedService.ShouldNotBeNull();
        savedService.Name.ShouldBe(createDto.Name);
    }

    [Fact]
    public async Task Create_WhenDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();

        var existingService = CreateServiceEntity(Guid.NewGuid());
        existingService.Name = createDto.Name;
        await _context.Services.AddAsync(existingService);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(createDto.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenServiceExists_ReturnsOkWithUpdatedService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdResult = await _controller.Create(createDto);
        var createdDto = ((CreatedAtActionResult)createdResult).Value.ShouldBeOfType<ServiceDto>();

        var updateDto = CreateUpdateServiceDto();

        // Act
        var result = await _controller.Update(createdDto.Id, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var updatedService = okResult.Value.ShouldBeOfType<ServiceDto>();
        updatedService.Id.ShouldBe(createdDto.Id);
        updatedService.Name.ShouldBe(updateDto.Name);

        var serviceInDb = await _context.Services.FindAsync(createdDto.Id);
        serviceInDb.ShouldNotBeNull();
        serviceInDb.Name.ShouldBe(updateDto.Name);
    }

    [Fact]
    public async Task Update_WhenServiceNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateDto = CreateUpdateServiceDto();

        // Act
        var result = await _controller.Update(Guid.NewGuid(), updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    [Fact]
    public async Task Update_WhenDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto1 = CreateCreateServiceDto();
        var createDto2 = CreateCreateServiceDto();
        createDto2.Name = "Different Name"; // Убедитесь, что имена разные

        var createdResult1 = await _controller.Create(createDto1);
        var createdDto1 = ((CreatedAtActionResult)createdResult1).Value.ShouldBeOfType<ServiceDto>();

        var createdResult2 = await _controller.Create(createDto2);
        var createdDto2 = ((CreatedAtActionResult)createdResult2).Value.ShouldBeOfType<ServiceDto>();

        var updateDto = CreateUpdateServiceDto();
        updateDto.Name = createDto1.Name;

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => _controller.Update(createdDto2.Id, updateDto));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenServiceExists_ReturnsNoContent()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdResult = await _controller.Create(createDto);
        var createdDto = ((CreatedAtActionResult)createdResult).Value.ShouldBeOfType<ServiceDto>();

        // Act
        var result = await _controller.Delete(createdDto.Id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        var deletedService = await _context.Services.FindAsync(createdDto.Id);
        deletedService.ShouldBeNull();
    }

    [Fact]
    public async Task Delete_WhenServiceNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Delete(Guid.NewGuid());

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region Helper Methods

    private static Service CreateServiceEntity(Guid? id = null, DateTimeOffset? endDate = null)
    {
        var serviceId = id ?? Guid.NewGuid();
        return new AutoFaker<Service>()
            .RuleFor(e => e.Id, _ => serviceId)
            .RuleFor(e => e.Name, f => f.Commerce.ProductName())
            .RuleFor(e => e.Description, f => f.Lorem.Sentence())
            .RuleFor(e => e.BeginDate, f => f.Date.PastOffset())
            .RuleFor(e => e.EndDate, _ => endDate)
            .RuleFor(e => e.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(e => e.CreatedBy, f => f.Person.FullName)
            .RuleFor(e => e.TariffPlans, _ => new List<TariffPlan>())
            .Generate();
    }

    private static List<Service> CreateServiceEntityList(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateServiceEntity())
            .ToList();
    }

    private static TariffPlan CreateTariffPlanEntity(Guid? id = null, Guid? serviceId = null)
    {
        var tariffId = id ?? Guid.NewGuid();
        return new AutoFaker<TariffPlan>()
            .RuleFor(e => e.Id, _ => tariffId)
            .RuleFor(e => e.Name, f => f.Commerce.ProductName())
            .RuleFor(e => e.Description, f => f.Lorem.Sentence())
            .RuleFor(e => e.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(e => e.ServiceId, _ => serviceId ?? Guid.NewGuid())
            .RuleFor(e => e.BeginDate, f => f.Date.PastOffset())
            .RuleFor(e => e.EndDate, _ => null)
            .Generate();
    }

    private static List<TariffPlan> CreateTariffPlanEntityList(int count, Guid? serviceId = null)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateTariffPlanEntity(serviceId: serviceId))
            .ToList();
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

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}