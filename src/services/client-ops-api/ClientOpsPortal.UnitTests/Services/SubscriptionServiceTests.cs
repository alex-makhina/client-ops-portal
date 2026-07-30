using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class SubscriptionServiceTests
{
    private readonly Mock<IGenericRepository<Subscription>> _subscriptionRepositoryMock;
    private readonly Mock<IGenericRepository<SubscriptionHistory>> _historyRepositoryMock;
    private readonly Mock<IDirectoryCacheService> _cacheMock;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _subscriptionRepositoryMock = new Mock<IGenericRepository<Subscription>>();
        _historyRepositoryMock = new Mock<IGenericRepository<SubscriptionHistory>>();
        _cacheMock = new Mock<IDirectoryCacheService>();
        _sut = new SubscriptionService(
            _subscriptionRepositoryMock.Object,
            _historyRepositoryMock.Object,
            _cacheMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenSubscriptionExists_ReturnsSubscriptionDto()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = CreateSubscriptionEntity(subscriptionId);

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _sut.GetByIdAsync(subscriptionId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        result.ContractId.ShouldBe(subscription.ContractId);

        _subscriptionRepositoryMock.Verify(
            r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubscriptionNotFound_ReturnsNull()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _sut.GetByIdAsync(subscriptionId, true);

        // Assert
        result.ShouldBeNull();

        _subscriptionRepositoryMock.Verify(
            r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenSubscriptionsExist_ReturnsListOfSubscriptionDtos()
    {
        // Arrange
        var subscriptions = CreateSubscriptionEntityList(5);
        var expectedCount = subscriptions.Count;

        _subscriptionRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _subscriptionRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoSubscriptionsExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Subscription>();

        _subscriptionRepositoryMock
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
    public async Task CreateAsync_WhenValidDto_CreatesSubscriptionAndHistory()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();

        _subscriptionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.ContractId.ShouldBe(createDto.ContractId);
        result.ServiceId.ShouldBe(createDto.ServiceId);

        _subscriptionRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _historyRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatesHistoryWithOpenActionAndPendingStatus()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();

        _subscriptionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();

        _historyRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistory>(h =>
                    h.ActionType == SubscriptionActionType.Open &&
                    h.Status == SubscriptionActionStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesSubscriptionAndReturnsDto()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = CreateSubscriptionEntity(subscriptionId);
        var updateDto = CreateUpdateSubscriptionDto();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(subscriptionId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenSubscriptionNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionDto();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(subscriptionId, updateDto));

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTariffPlanIdWhenProvided()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = CreateSubscriptionEntity(subscriptionId);
        var newTariffPlanId = Guid.NewGuid();
        var updateDto = new UpdateSubscriptionDto
        {
            TariffPlanId = newTariffPlanId,
            EndDate = null
        };

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(subscriptionId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Subscription>(s => s.TariffPlanId == newTariffPlanId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEndDateWhenProvided()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = CreateSubscriptionEntity(subscriptionId);
        var newEndDate = DateTimeOffset.UtcNow.AddDays(30);
        var updateDto = new UpdateSubscriptionDto
        {
            TariffPlanId = null,
            EndDate = newEndDate
        };

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(subscriptionId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Subscription>(s => s.EndDate == newEndDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenSubscriptionExists_CallsRepositoryDelete()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(r => r.DeleteAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(subscriptionId);

        // Assert
        _subscriptionRepositoryMock.Verify(
            r => r.DeleteAsync(subscriptionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredSubscriptions()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var subscriptions = CreateSubscriptionEntityList(3);
        foreach (var subscription in subscriptions)
        {
            subscription.ContractId = contractId;
        }

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetWhereAsync(s => s.ContractId == contractId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(subscriptions.Count);

        _subscriptionRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<Subscription, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetActiveSubscriptionsByContractAsync Tests

    [Fact]
    public async Task GetActiveSubscriptionsByContractAsync_WhenActiveSubscriptionsExist_ReturnsFullDataDtos()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var subscriptions = CreateSubscriptionEntityList(3, contractId: contractId);
        foreach (var subscription in subscriptions)
        {
            subscription.EndDate = null;
        }

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetActiveSubscriptionsByContractAsync(contractId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(subscriptions.Count);

        _subscriptionRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionsByContractAsync_WhenNoActiveSubscriptions_ReturnsEmptyList()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var emptyList = new List<Subscription>();

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetActiveSubscriptionsByContractAsync(contractId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetActiveSubscriptionsByContractAsync_ExcludesExpiredSubscriptions()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var activeSubscription = CreateSubscriptionEntity(Guid.NewGuid(), contractId: contractId);
        activeSubscription.EndDate = null;

        var expiredSubscription = CreateSubscriptionEntity(Guid.NewGuid(), contractId: contractId);
        expiredSubscription.EndDate = DateTimeOffset.UtcNow.AddDays(-1);

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { activeSubscription });

        // Act
        var result = await _sut.GetActiveSubscriptionsByContractAsync(contractId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    #endregion

    #region GetFullSubscriptionDataAsync Tests

    [Fact]
    public async Task GetFullSubscriptionDataAsync_WhenSubscriptionExists_ReturnsFullDataDto()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = CreateFullSubscriptionEntity(subscriptionId);

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _sut.GetFullSubscriptionDataAsync(subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.ServiceName.ShouldBe(subscription.Service!.Name);
        result.TariffPlanName.ShouldBe(subscription.TariffPlan!.Name);

        _subscriptionRepositoryMock.Verify(
            r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFullSubscriptionDataAsync_WhenSubscriptionNotFound_ReturnsNull()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _sut.GetFullSubscriptionDataAsync(subscriptionId);

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region ChangeTariffPlanAsync Tests

    [Fact]
    public async Task ChangeTariffPlanAsync_WhenSubscriptionExists_ChangesTariffAndCreatesHistory()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = CreateSubscriptionEntity(subscriptionId);
        var oldTariffPlanId = existingSubscription.TariffPlanId;
        var newTariffPlanId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ChangeTariffPlanAsync(subscriptionId, newTariffPlanId);

        // Assert
        result.ShouldNotBeNull();

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Subscription>(s => s.TariffPlanId == newTariffPlanId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _historyRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistory>(h =>
                    h.ActionType == SubscriptionActionType.TariffChange &&
                    h.Status == SubscriptionActionStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeTariffPlanAsync_WhenSubscriptionNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var newTariffPlanId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.ChangeTariffPlanAsync(subscriptionId, newTariffPlanId));

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region CancelSubscriptionAsync Tests

    [Fact]
    public async Task CancelSubscriptionAsync_WhenSubscriptionExists_SetsEndDateAndCreatesHistory()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var existingSubscription = CreateSubscriptionEntity(subscriptionId);
        existingSubscription.EndDate = null;

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CancelSubscriptionAsync(subscriptionId);

        // Assert
        result.ShouldNotBeNull();

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Subscription>(s => s.EndDate != null),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _historyRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistory>(h =>
                    h.ActionType == SubscriptionActionType.Close &&
                    h.Status == SubscriptionActionStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_WhenSubscriptionNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.CancelSubscriptionAsync(subscriptionId));

        _subscriptionRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GetSubscriptionsByAbonentIdAsync Tests

    [Fact]
    public async Task GetSubscriptionsByAbonentIdAsync_WhenSubscriptionsExist_ReturnsFullDataDtos()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var subscriptions = CreateSubscriptionEntityList(3, abonentId: abonentId);

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsByAbonentIdAsync(abonentId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(subscriptions.Count);

        _subscriptionRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsByAbonentIdAsync_WhenOnlyActiveFalse_ReturnsAllSubscriptions()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var activeSubscription = CreateSubscriptionEntity(Guid.NewGuid(), abonentId: abonentId);
        activeSubscription.EndDate = null;

        var expiredSubscription = CreateSubscriptionEntity(Guid.NewGuid(), abonentId: abonentId);
        expiredSubscription.EndDate = DateTimeOffset.UtcNow.AddDays(-1);

        var subscriptions = new List<Subscription> { activeSubscription, expiredSubscription };

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _sut.GetSubscriptionsByAbonentIdAsync(abonentId, false);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetSubscriptionsByAbonentIdAsync_WhenOnlyActiveTrue_ReturnsOnlyActiveSubscriptions()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var activeSubscription = CreateSubscriptionEntity(Guid.NewGuid(), abonentId: abonentId);
        activeSubscription.EndDate = null;

        var expiredSubscription = CreateSubscriptionEntity(Guid.NewGuid(), abonentId: abonentId);
        expiredSubscription.EndDate = DateTimeOffset.UtcNow.AddDays(-1);

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { activeSubscription });

        // Act
        var result = await _sut.GetSubscriptionsByAbonentIdAsync(abonentId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetSubscriptionsByAbonentIdAsync_WhenNoSubscriptions_ReturnsEmptyList()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var emptyList = new List<Subscription>();

        _subscriptionRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Subscription, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetSubscriptionsByAbonentIdAsync(abonentId, true);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region CreateHistory Tests

    [Fact]
    public void CreateHistory_CreatesHistoryWithOneStep()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();

        // Act
        var result = _sut.CreateHistory(subscriptionId, SubscriptionActionType.Open, SubscriptionActionStatus.Pending, tariffId);

        // Assert
        result.ShouldNotBeNull();
        result.SubscriptionId.ShouldBe(subscriptionId);
        result.ActionType.ShouldBe(SubscriptionActionType.Open);
        result.Status.ShouldBe(SubscriptionActionStatus.Pending);
        result.TariffPlanId.ShouldBe(tariffId);
        result.Steps.ShouldNotBeNull();
        result.Steps.Count.ShouldBe(1);
        result.Steps[0].Status.ShouldBe(SubscriptionActionStatus.Pending);
    }

    [Fact]
    public void CreateHistory_CreatesHistoryWithDifferentActionTypes()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();

        // Act
        var openHistory = _sut.CreateHistory(subscriptionId, SubscriptionActionType.Open, SubscriptionActionStatus.Pending, tariffId);
        var closeHistory = _sut.CreateHistory(subscriptionId, SubscriptionActionType.Close, SubscriptionActionStatus.Completed, tariffId);
        var tariffChangeHistory = _sut.CreateHistory(subscriptionId, SubscriptionActionType.TariffChange, SubscriptionActionStatus.InProgress, tariffId);

        // Assert
        openHistory.ActionType.ShouldBe(SubscriptionActionType.Open);
        closeHistory.ActionType.ShouldBe(SubscriptionActionType.Close);
        tariffChangeHistory.ActionType.ShouldBe(SubscriptionActionType.TariffChange);
    }

    #endregion

    #region Helper Methods

    private static Contract CreateContract(Guid? abonentId = null, string? contractNumber = null)
    {
        return new Contract
        {
            Id = Guid.NewGuid(),
            AbonentId = abonentId ?? Guid.NewGuid(),
            ContractNumber = contractNumber ?? $"CT-{DateTimeOffset.UtcNow:yyyyMMdd}-{new Random().Next(1, 9999):D4}",
            BeginDate = DateTimeOffset.UtcNow,
            EndDate = null
        };
    }

    private static Service CreateService(string? name = null, string? description = null)
    {
        return new Service
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Service-{Guid.NewGuid():N}",
            Description = description ?? $"Description-{Guid.NewGuid():N}",
            BeginDate = DateTimeOffset.UtcNow,
            EndDate = null
        };
    }

    private static TariffPlan CreateTariffPlan(Guid? serviceId = null, string? name = null, decimal? price = null)
    {
        return new TariffPlan
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId ?? Guid.NewGuid(),
            Name = name ?? $"Tariff-{Guid.NewGuid():N}",
            Description = $"Tariff Description-{Guid.NewGuid():N}",
            Price = price ?? new Random().Next(100, 1000),
            BeginDate = DateTimeOffset.UtcNow,
            EndDate = null
        };
    }

    private static Subscription CreateSubscriptionEntity(Guid? id = null, Guid? contractId = null, Guid? abonentId = null)
    {
        var service = CreateService();
        var tariffPlan = CreateTariffPlan(service.Id);
        var contract = CreateContract(abonentId);

        var subscription = new AutoFaker<Subscription>()
            .RuleFor(s => s.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(s => s.ContractId, _ => contractId ?? contract.Id)
            .RuleFor(s => s.ServiceId, _ => service.Id)
            .RuleFor(s => s.TariffPlanId, _ => tariffPlan.Id)
            .RuleFor(s => s.BeginDate, _ => DateTimeOffset.UtcNow)
            .RuleFor(s => s.EndDate, _ => null)
            .RuleFor(s => s.CreatedAt, _ => DateTimeOffset.UtcNow)
            .RuleFor(s => s.UpdatedAt, _ => DateTimeOffset.UtcNow)
            .Generate();

        subscription.Service = service;
        subscription.TariffPlan = tariffPlan;
        subscription.Contract = contract;

        return subscription;
    }

    private static Subscription CreateFullSubscriptionEntity(Guid? id = null)
    {
        var service = CreateService();
        var tariffPlan = CreateTariffPlan(service.Id);
        var contract = CreateContract();

        var subscription = CreateSubscriptionEntity(id);
        subscription.Service = service;
        subscription.ServiceId = service.Id;
        subscription.TariffPlan = tariffPlan;
        subscription.TariffPlanId = tariffPlan.Id;
        subscription.Contract = contract;
        subscription.ContractId = contract.Id;

        return subscription;
    }

    private static List<Subscription> CreateSubscriptionEntityList(int count, Guid? contractId = null, Guid? abonentId = null)
    {
        var list = new List<Subscription>();
        for (int i = 0; i < count; i++)
        {
            var subscription = CreateSubscriptionEntity(abonentId: abonentId);
            if (contractId.HasValue)
                subscription.ContractId = contractId.Value;
            list.Add(subscription);
        }
        return list;
    }

    private static SubscriptionDto CreateSubscriptionDto()
    {
        return new AutoFaker<SubscriptionDto>().Generate();
    }

    private static UpdateSubscriptionDto CreateUpdateSubscriptionDto()
    {
        return new AutoFaker<UpdateSubscriptionDto>().Generate();
    }

    #endregion
}