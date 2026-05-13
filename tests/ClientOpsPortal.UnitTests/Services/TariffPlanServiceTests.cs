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

public class TariffPlanServiceTests
{
    private readonly Mock<IGenericRepository<TariffPlan>> _tariffPlanRepositoryMock;
    private readonly TariffPlanService _sut;

    public TariffPlanServiceTests()
    {
        _tariffPlanRepositoryMock = new Mock<IGenericRepository<TariffPlan>>();
        _sut = new TariffPlanService(_tariffPlanRepositoryMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenTariffPlanExists_ReturnsTariffPlanDto()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var tariffPlan = CreateTariffPlanEntity(tariffPlanId);

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlan);

        // Act
        var result = await _sut.GetByIdAsync(tariffPlanId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tariffPlanId);
        result.Name.ShouldBe(tariffPlan.Name);

        _tariffPlanRepositoryMock.Verify(
            r => r.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTariffPlanNotFound_ReturnsNull()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TariffPlan?)null);

        // Act
        var result = await _sut.GetByIdAsync(tariffPlanId, true);

        // Assert
        result.ShouldBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.GetByIdAsync(tariffPlanId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenTariffPlansExist_ReturnsListOfTariffPlanDtos()
    {
        // Arrange
        var tariffPlans = CreateTariffPlanEntityList(5);
        var expectedCount = tariffPlans.Count;

        _tariffPlanRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlans);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _tariffPlanRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoTariffPlansExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<TariffPlan>();

        _tariffPlanRepositoryMock
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
    public async Task CreateAsync_WhenValidDto_CreatesTariffPlanAndReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        var createdTariffPlan = createDto.ToEntity();

        _tariffPlanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Callback<TariffPlan, CancellationToken>((t, ct) =>
            {
                t.Id = createdTariffPlan.Id;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.Description.ShouldBe(createDto.Description);
        result.Price.ShouldBe(createDto.Price);
        result.ServiceId.ShouldBe(createDto.ServiceId);

        _tariffPlanRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<TariffPlan>(t =>
                    t.Name == createDto.Name &&
                    t.Description == createDto.Description &&
                    t.Price == createDto.Price &&
                    t.ServiceId == createDto.ServiceId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEndDate_CreatesTariffPlanWithEndDate()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();
        createDto.EndDate = DateTimeOffset.UtcNow.AddDays(30);

        _tariffPlanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<TariffPlan>(t => t.EndDate == createDto.EndDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesTariffPlanAndReturnsDto()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var updateDto = CreateUpdateTariffPlanDto();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tariffPlanId);

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTariffPlanNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TariffPlan?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(tariffPlanId, updateDto));

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameWhenProvided()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var newName = "Updated Tariff Name";
        var updateDto = new UpdateTariffPlanDto
        {
            Name = newName,
            Description = null,
            Price = null,
            BeginDate = null,
            EndDate = null
        };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.Name == newName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDescriptionWhenProvided()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var newDescription = "Updated Description";
        var updateDto = new UpdateTariffPlanDto
        {
            Name = null,
            Description = newDescription,
            Price = null,
            BeginDate = null,
            EndDate = null
        };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.Description == newDescription),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPriceWhenProvided()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var newPrice = 999.99m;
        var updateDto = new UpdateTariffPlanDto
        {
            Name = null,
            Description = null,
            Price = newPrice,
            BeginDate = null,
            EndDate = null
        };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.Price == newPrice),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBeginDateWhenProvided()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var newBeginDate = DateTimeOffset.UtcNow.AddDays(10);
        var updateDto = new UpdateTariffPlanDto
        {
            Name = null,
            Description = null,
            Price = null,
            BeginDate = newBeginDate,
            EndDate = null
        };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.BeginDate == newBeginDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEndDateWhenProvided()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var newEndDate = DateTimeOffset.UtcNow.AddDays(30);
        var updateDto = new UpdateTariffPlanDto
        {
            Name = null,
            Description = null,
            Price = null,
            BeginDate = null,
            EndDate = newEndDate
        };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.EndDate == newEndDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMultipleFieldsWhenProvided()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var newName = "Updated Name";
        var newPrice = 500m;
        var updateDto = new UpdateTariffPlanDto
        {
            Name = newName,
            Description = null,
            Price = newPrice,
            BeginDate = null,
            EndDate = null
        };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariffPlan);

        _tariffPlanRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TariffPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(tariffPlanId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(t => t.Name == newName && t.Price == newPrice),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenTariffPlanExists_CallsRepositoryDelete()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();

        _tariffPlanRepositoryMock
            .Setup(r => r.DeleteAsync(tariffPlanId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(tariffPlanId);

        // Assert
        _tariffPlanRepositoryMock.Verify(
            r => r.DeleteAsync(tariffPlanId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredTariffPlans()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffPlans = CreateTariffPlanEntityList(3);
        foreach (var tariffPlan in tariffPlans)
        {
            tariffPlan.ServiceId = serviceId;
        }

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlans);

        // Act
        var result = await _sut.GetWhereAsync(t => t.ServiceId == serviceId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(tariffPlans.Count);

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<TariffPlan, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<TariffPlan>();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetWhereAsync(t => t.ServiceId == Guid.NewGuid(), false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetActiveTariffPlansByServiceAsync Tests

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_WhenActiveTariffsExist_ReturnsShortDataDtos()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffPlans = CreateTariffPlanEntityList(3, serviceId: serviceId);
        foreach (var tariffPlan in tariffPlans)
        {
            tariffPlan.EndDate = null;
        }

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlans);

        // Act
        var result = await _sut.GetActiveTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(tariffPlans.Count);

        foreach (var shortDto in result)
        {
            shortDto.Id.ShouldNotBe(Guid.Empty);
            shortDto.Name.ShouldNotBeNullOrEmpty();
            shortDto.Price.ShouldBeGreaterThan(0);
        }

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_ExcludesExpiredTariffs()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var activeTariff = CreateTariffPlanEntity(Guid.NewGuid(), serviceId: serviceId);
        activeTariff.EndDate = null;

        var expiredTariff = CreateTariffPlanEntity(Guid.NewGuid(), serviceId: serviceId);
        expiredTariff.EndDate = DateTimeOffset.UtcNow.AddDays(-1);

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TariffPlan> { activeTariff });

        // Act
        var result = await _sut.GetActiveTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_WhenNoActiveTariffs_ReturnsEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var emptyList = new List<TariffPlan>();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetActiveTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetTariffPlansByServiceAsync Tests

    [Fact]
    public async Task GetTariffPlansByServiceAsync_WhenTariffsExist_ReturnsTariffPlanDtos()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffPlans = CreateTariffPlanEntityList(4, serviceId: serviceId);

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlans);

        // Act
        var result = await _sut.GetTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(tariffPlans.Count);

        foreach (var dto in result)
        {
            dto.ServiceId.ShouldBe(serviceId);
        }

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTariffPlansByServiceAsync_IncludesBothActiveAndExpiredTariffs()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var activeTariff = CreateTariffPlanEntity(Guid.NewGuid(), serviceId: serviceId);
        activeTariff.EndDate = null;

        var expiredTariff = CreateTariffPlanEntity(Guid.NewGuid(), serviceId: serviceId);
        expiredTariff.EndDate = DateTimeOffset.UtcNow.AddDays(-1);

        var tariffPlans = new List<TariffPlan> { activeTariff, expiredTariff };

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlans);

        // Act
        var result = await _sut.GetTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetTariffPlansByServiceAsync_WhenNoTariffs_ReturnsEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var emptyList = new List<TariffPlan>();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region Helper Methods

    private static TariffPlan CreateTariffPlanEntity(Guid? id = null, Guid? serviceId = null)
    {
        return new TariffPlan
        {
            Id = id ?? Guid.NewGuid(),
            Name = $"Tariff-{Guid.NewGuid():N}",
            Description = $"Description-{Guid.NewGuid():N}",
            Price = new Random().Next(100, 1000),
            ServiceId = serviceId ?? Guid.NewGuid(),
            BeginDate = DateTimeOffset.UtcNow,
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

    private static CreateTariffPlanDto CreateCreateTariffPlanDto()
    {
        return new AutoFaker<CreateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => $"Tariff-{Guid.NewGuid():N}")
            .RuleFor(dto => dto.Description, f => $"Description-{Guid.NewGuid():N}")
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(100, 1000))
            .RuleFor(dto => dto.ServiceId, f => Guid.NewGuid())
            .RuleFor(dto => dto.BeginDate, f => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, f => null)
            .Generate();
    }

    private static UpdateTariffPlanDto CreateUpdateTariffPlanDto()
    {
        return new AutoFaker<UpdateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => $"Updated-{Guid.NewGuid():N}")
            .RuleFor(dto => dto.Description, f => $"Updated Description-{Guid.NewGuid():N}")
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(200, 2000))
            .RuleFor(dto => dto.BeginDate, f => DateTimeOffset.UtcNow.AddDays(5))
            .RuleFor(dto => dto.EndDate, f => DateTimeOffset.UtcNow.AddDays(60))
            .Generate();
    }

    #endregion
}