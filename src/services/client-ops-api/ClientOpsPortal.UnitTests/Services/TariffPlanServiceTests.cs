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

public class TariffPlanServiceTests : IDisposable
{
    private readonly Mock<IGenericRepository<TariffPlan>> _tariffPlanRepositoryMock;
    private readonly TariffPlanService _sut;

    public TariffPlanServiceTests()
    {
        _tariffPlanRepositoryMock = new Mock<IGenericRepository<TariffPlan>>();
        _sut = new TariffPlanService(_tariffPlanRepositoryMock.Object);

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });
    }

    public void Dispose()
    {
        _tariffPlanRepositoryMock.Reset();
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
        result.Price.ShouldBe(tariffPlan.Price);

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

    [Fact]
    public async Task GetByIdAsync_WithIncludesFalse_PassesParameterToRepository()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var tariffPlan = CreateTariffPlanEntity(tariffPlanId);

        _tariffPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlan);

        // Act
        var result = await _sut.GetByIdAsync(tariffPlanId, false);

        // Assert
        result.ShouldNotBeNull();

        _tariffPlanRepositoryMock.Verify(
            r => r.GetByIdAsync(tariffPlanId, false, It.IsAny<CancellationToken>()),
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
            .Callback<TariffPlan, CancellationToken>((tp, ct) =>
            {
                tp.Id = createdTariffPlan.Id;
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
                It.Is<TariffPlan>(tp =>
                    tp.Name == createDto.Name &&
                    tp.Description == createDto.Description &&
                    tp.Price == createDto.Price &&
                    tp.ServiceId == createDto.ServiceId),
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
    public async Task UpdateAsync_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var tariffPlanId = Guid.NewGuid();
        var existingTariffPlan = CreateTariffPlanEntity(tariffPlanId);
        var originalName = existingTariffPlan.Name;
        var originalDescription = existingTariffPlan.Description;
        var originalPrice = existingTariffPlan.Price;

        var updateDto = new UpdateTariffPlanDto
        {
            Name = "Updated Name",
            Price = 999.99m
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
        result.Id.ShouldBe(tariffPlanId);
        result.Name.ShouldBe(updateDto.Name);
        result.Price.ShouldBe(updateDto.Price.Value);
        result.Description.ShouldBe(originalDescription);

        _tariffPlanRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<TariffPlan>(tp =>
                    tp.Name == updateDto.Name &&
                    tp.Price == updateDto.Price.Value &&
                    tp.Description == originalDescription),
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
        foreach (var tp in tariffPlans)
        {
            tp.ServiceId = serviceId;
        }

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffPlans);

        // Act
        var result = await _sut.GetWhereAsync(tp => tp.ServiceId == serviceId, true);

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
        var result = await _sut.GetWhereAsync(tp => tp.ServiceId == Guid.NewGuid(), false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetActiveTariffPlansByServiceAsync Tests

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_WhenTariffPlansExist_ReturnsActiveTariffPlans()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffPlans = CreateTariffPlanEntityList(3);
        foreach (var tp in tariffPlans)
        {
            tp.ServiceId = serviceId;
            tp.EndDate = null; 
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
        result.Count.ShouldBe(3);

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_WhenNoTariffPlansExist_ReturnsEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TariffPlan>());

        // Act
        var result = await _sut.GetActiveTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetTariffPlansByServiceAsync Tests

    [Fact]
    public async Task GetTariffPlansByServiceAsync_WhenTariffPlansExist_ReturnsTariffPlans()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var tariffPlans = CreateTariffPlanEntityList(3);
        foreach (var tp in tariffPlans)
        {
            tp.ServiceId = serviceId;
        }

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
        result.Count.ShouldBe(3);

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTariffPlansByServiceAsync_WhenNoTariffPlansExist_ReturnsEmptyList()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        _tariffPlanRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TariffPlan>());

        // Act
        var result = await _sut.GetTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTariffPlansByServiceAsync_ReturnsAllTariffPlansIncludingInactive()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var activeTariffPlan = CreateTariffPlanEntity(Guid.NewGuid());
        activeTariffPlan.ServiceId = serviceId;
        activeTariffPlan.EndDate = null;

        var expiredTariffPlan = CreateTariffPlanEntity(Guid.NewGuid());
        expiredTariffPlan.ServiceId = serviceId;
        expiredTariffPlan.EndDate = now.AddDays(-1);

        var tariffPlans = new List<TariffPlan> { activeTariffPlan, expiredTariffPlan };

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

        _tariffPlanRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<TariffPlan, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static TariffPlan CreateTariffPlanEntity(Guid? id = null)
    {
        var faker = new AutoFaker<TariffPlan>();

        if (id.HasValue)
            faker.RuleFor(tp => tp.Id, _ => id.Value);

        return faker
            .RuleFor(tp => tp.Name, f => f.Commerce.ProductName())
            .RuleFor(tp => tp.Description, f => f.Lorem.Sentence())
            .RuleFor(tp => tp.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(tp => tp.ServiceId, _ => Guid.NewGuid())
            .RuleFor(tp => tp.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(tp => tp.EndDate, _ => null)
            .Generate();
    }

    private static List<TariffPlan> CreateTariffPlanEntityList(int count)
    {
        var list = new List<TariffPlan>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateTariffPlanEntity());
        }
        return list;
    }

    private static CreateTariffPlanDto CreateCreateTariffPlanDto()
    {
        return new AutoFaker<CreateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, _ => Guid.NewGuid())
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static UpdateTariffPlanDto CreateUpdateTariffPlanDto()
    {
        return new AutoFaker<UpdateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    #endregion
}