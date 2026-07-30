using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class ServiceServiceTests : IDisposable
{
    private readonly Mock<IGenericRepository<Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<TariffPlan>> _tariffPlanRepositoryMock;
    private readonly ServiceService _sut;

    public ServiceServiceTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Service>>();
        _tariffPlanRepositoryMock = new Mock<IGenericRepository<TariffPlan>>();
        _sut = new ServiceService(
            _serviceRepositoryMock.Object,
            _tariffPlanRepositoryMock.Object);

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });
    }

    public void Dispose()
    {
        _serviceRepositoryMock.Reset();
        _tariffPlanRepositoryMock.Reset();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenServiceExists_ReturnsServiceDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var service = CreateServiceEntity(serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetByIdAsync(serviceId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
        result.Name.ShouldBe(service.Name);

        _serviceRepositoryMock.Verify(
            r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenServiceNotFound_ReturnsNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.GetByIdAsync(serviceId, true);

        // Assert
        result.ShouldBeNull();

        _serviceRepositoryMock.Verify(
            r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludesFalse_PassesParameterToRepository()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var service = CreateServiceEntity(serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetByIdAsync(serviceId, false);

        // Assert
        result.ShouldNotBeNull();

        _serviceRepositoryMock.Verify(
            r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenServicesExist_ReturnsListOfServiceDtos()
    {
        // Arrange
        var services = CreateServiceEntityList(5);
        var expectedCount = services.Count;

        _serviceRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _serviceRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoServicesExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Service>();

        _serviceRepositoryMock
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAllAsync(false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidDto_CreatesServiceAndReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var createdService = createDto.ToEntity();

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        _serviceRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Callback<Service, CancellationToken>((s, ct) =>
            {
                s.Id = createdService.Id;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.Description.ShouldBe(createDto.Description);

        _serviceRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Service>(s =>
                    s.Name == createDto.Name &&
                    s.Description == createDto.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();
        var existingService = CreateServiceEntity(Guid.NewGuid());
        existingService.Name = createDto.Name;

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { existingService });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateAsync(createDto));

        exception.Message.ShouldContain(createDto.Name);

        _serviceRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesServiceAndReturnsDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var existingService = CreateServiceEntity(serviceId);
        var updateDto = CreateUpdateServiceDto();

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        _serviceRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(serviceId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);

        _serviceRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenServiceNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(serviceId, updateDto));

        _serviceRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameChangedAndNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var existingService = CreateServiceEntity(serviceId);
        var updateDto = CreateUpdateServiceDto();
        var otherService = CreateServiceEntity(Guid.NewGuid());
        otherService.Name = updateDto.Name;

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { otherService });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.UpdateAsync(serviceId, updateDto));

        exception.Message.ShouldContain(updateDto.Name);

        _serviceRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithTariffPlans_UpdatesTariffPlans()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var existingService = CreateServiceEntity(serviceId);
        var updateDto = CreateUpdateServiceDtoWithTariffPlans();

        var existingTariffs = new List<TariffPlan>
        {
            CreateTariffPlanEntity(Guid.NewGuid(), serviceId),
            CreateTariffPlanEntity(Guid.NewGuid(), serviceId)
        };

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingService);

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffs);

        _serviceRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _tariffPlanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _tariffPlanRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(serviceId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenServiceExists_CallsRepositoryDelete()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceRepositoryMock
            .Setup(r => r.DeleteAsync(serviceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(serviceId);

        // Assert
        _serviceRepositoryMock.Verify(
            r => r.DeleteAsync(serviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredServices()
    {
        // Arrange
        var services = CreateServiceEntityList(3);

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.GetWhereAsync(s => s.Id == services.First().Id, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(services.Count);

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<Service, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Service>();

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetWhereAsync(s => s.Id == Guid.NewGuid(), false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetFullServiceDataAsync Tests

    [Fact]
    public async Task GetFullServiceDataAsync_WhenServiceExists_ReturnsFullServiceDataDto()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var service = CreateServiceEntity(serviceId);
        service.TariffPlans = CreateTariffPlanEntityList(3, serviceId);

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetFullServiceDataAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
        result.Name.ShouldBe(service.Name);
        result.TariffPlans.ShouldNotBeNull();
        result.TariffPlans.Count.ShouldBe(3);

        _serviceRepositoryMock.Verify(
            r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFullServiceDataAsync_WhenServiceNotFound_ReturnsNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _serviceRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.GetFullServiceDataAsync(serviceId);

        // Assert
        result.ShouldBeNull();

        _serviceRepositoryMock.Verify(
            r => r.GetByIdAsync(serviceId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveServicesAsync Tests

    [Fact]
    public async Task GetActiveServicesAsync_WhenServicesExist_ReturnsActiveServices()
    {
        // Arrange
        var services = CreateServiceEntityList(3);
        foreach (var service in services)
        {
            service.EndDate = null; // Активная услуга
        }

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _sut.GetActiveServicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveServicesAsync_WhenNoServicesExist_ReturnsEmptyList()
    {
        // Arrange
        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        // Act
        var result = await _sut.GetActiveServicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region IsServiceNameUniqueAsync Tests

    [Fact]
    public async Task IsServiceNameUniqueAsync_WhenNameIsUnique_ReturnsTrue()
    {
        // Arrange
        var name = "Unique Service Name";

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        // Act
        var result = await _sut.IsServiceNameUniqueAsync(name);

        // Assert
        result.ShouldBeTrue();

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsServiceNameUniqueAsync_WhenNameIsNotUnique_ReturnsFalse()
    {
        // Arrange
        var name = "Existing Service Name";
        var existingService = CreateServiceEntity(Guid.NewGuid());
        existingService.Name = name;

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { existingService });

        // Act
        var result = await _sut.IsServiceNameUniqueAsync(name);

        // Assert
        result.ShouldBeFalse();

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsServiceNameUniqueAsync_WithExcludeId_ReturnsTrueWhenOnlyExcludedExists()
    {
        // Arrange
        var name = "Existing Service Name";
        var excludeId = Guid.NewGuid();
        var existingService = CreateServiceEntity(excludeId);
        existingService.Name = name;

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { existingService });

        // Act
        var result = await _sut.IsServiceNameUniqueAsync(name, excludeId);

        // Assert
        result.ShouldBeTrue();

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsServiceNameUniqueAsync_WithExcludeId_ReturnsFalseWhenOtherExists()
    {
        // Arrange
        var name = "Existing Service Name";
        var excludeId = Guid.NewGuid();
        var existingService1 = CreateServiceEntity(excludeId);
        existingService1.Name = name;
        var existingService2 = CreateServiceEntity(Guid.NewGuid());
        existingService2.Name = name;

        _serviceRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { existingService1, existingService2 });

        // Act
        var result = await _sut.IsServiceNameUniqueAsync(name, excludeId);

        // Assert
        result.ShouldBeFalse();

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task IsServiceNameUniqueAsync_WhenNameIsEmpty_ReturnsFalse(string? name)
    {
        // Act
        var result = await _sut.IsServiceNameUniqueAsync(name!);

        // Assert
        result.ShouldBeFalse();

        _serviceRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private static Service CreateServiceEntity(Guid? id = null)
    {
        var faker = new AutoFaker<Service>();

        if (id.HasValue)
            faker.RuleFor(s => s.Id, _ => id.Value);

        return faker
            .RuleFor(s => s.Name, f => f.Commerce.ProductName())
            .RuleFor(s => s.Description, f => f.Lorem.Sentence())
            .RuleFor(s => s.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(s => s.EndDate, _ => null)
            .RuleFor(s => s.TariffPlans, _ => new List<TariffPlan>())
            .Generate();
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
        var faker = new AutoFaker<TariffPlan>();

        if (id.HasValue)
            faker.RuleFor(t => t.Id, _ => id.Value);

        if (serviceId.HasValue)
            faker.RuleFor(t => t.ServiceId, _ => serviceId.Value);

        return faker
            .RuleFor(t => t.Name, f => f.Commerce.ProductName())
            .RuleFor(t => t.Description, f => f.Lorem.Sentence())
            .RuleFor(t => t.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(t => t.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(t => t.EndDate, _ => null)
            .Generate();
    }

    private static List<TariffPlan> CreateTariffPlanEntityList(int count, Guid serviceId)
    {
        var list = new List<TariffPlan>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanEntity(Guid.NewGuid(), serviceId));
        }
        return list;
    }

    private static CreateServiceDto CreateCreateServiceDto()
    {
        return new AutoFaker<CreateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<CreateTariffPlanDto>())
            .Generate();
    }

    private static UpdateServiceDto CreateUpdateServiceDto()
    {
        return new AutoFaker<UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => null)
            .Generate();
    }

    private static UpdateServiceDto CreateUpdateServiceDtoWithTariffPlans()
    {
        var tariffPlanItems = new List<UpdateTariffPlanFromServiceDto>
        {
            new AutoFaker<UpdateTariffPlanFromServiceDto>()
                .RuleFor(dto => dto.Id, _ => Guid.NewGuid())
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
                .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
                .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
                .RuleFor(dto => dto.EndDate, _ => null)
                .Generate(),
            new AutoFaker<UpdateTariffPlanFromServiceDto>()
                .RuleFor(dto => dto.Id, _ => Guid.Empty) 
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
                .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
                .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
                .RuleFor(dto => dto.EndDate, _ => null)
                .Generate()
        };

        return new AutoFaker<UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => tariffPlanItems)
            .Generate();
    }

    #endregion
}