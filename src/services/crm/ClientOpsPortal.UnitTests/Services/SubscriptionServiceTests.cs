using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Services.Reporting.Contracts.Events;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using MassTransit;
using Moq;
using Shouldly;
using System.Linq.Expressions;
using Xunit;
using AppSubscriptionHistoryDto = ClientOpsPortal.Application.DTOs.SubscriptionHistoryDto;
using AppSubscriptionHistoryStepDto = ClientOpsPortal.Application.DTOs.SubscriptionHistoryStepDto;
using ContractSubscriptionHistoryStepDto = ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs.SubscriptionHistoryStepDto;
using CreateStepDto = ClientOpsPortal.Application.DTOs.CreateSubscriptionHistoryStepDto;
using StepDto = ClientOpsPortal.Application.DTOs.SubscriptionHistoryStepDto;
using SubscriptionHistoryStepModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistoryStep;

namespace ClientOpsPortal.UnitTests.Services;

public class SubscriptionServiceTests
{
    private readonly Mock<IGenericRepository<Subscription>> _subscriptionRepositoryMock;
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<ISubscriptionHistoryClient> _historyClientMock;
    private readonly Mock<IDirectoryCacheService> _cacheMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _subscriptionRepositoryMock = new Mock<IGenericRepository<Subscription>>();
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _historyClientMock = new Mock<ISubscriptionHistoryClient>();
        _cacheMock = new Mock<IDirectoryCacheService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _cacheMock
            .Setup(x => x.GetServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new ClientOpsPortal.Services.Directory.Contracts.DTOs.ServiceDto
            {
                Id = id,
                Name = $"Service {id:N}",
                Description = $"Description for service {id:N}",
                BeginDate = DateTimeOffset.UtcNow,
                EndDate = null
            });

        _cacheMock
            .Setup(x => x.GetTariffPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new ClientOpsPortal.Services.Directory.Contracts.DTOs.TariffPlanDto
            {
                Id = id,
                Name = $"Tariff {id:N}",
                Description = $"Description for tariff {id:N}",
                Price = 100,
                ServiceId = Guid.NewGuid(),
                BeginDate = DateTimeOffset.UtcNow,
                EndDate = null
            });

        _historyClientMock
            .Setup(x => x.CreateHistoryAsync(It.IsAny<SubscriptionHistoryEventDto>(), It.IsAny<CancellationToken>()))
            .Returns<SubscriptionHistoryEventDto, CancellationToken>((dto, ct) =>
                Task.FromResult(new AppSubscriptionHistoryDto
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = dto.SubscriptionId,
                    ActionType = (Domain.Enums.SubscriptionActionType)dto.ActionType,
                    Status = (Domain.Enums.SubscriptionActionStatus)dto.Status,
                    TariffPlanId = dto.TariffPlanId,
                    StartDate = DateTimeOffset.UtcNow,
                    Steps = new List<AppSubscriptionHistoryStepDto>()
                }));

        _historyClientMock
            .Setup(x => x.UpdateHistoryStatusAsync(It.IsAny<Guid>(), It.IsAny<Domain.Enums.SubscriptionActionStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historyClientMock
            .Setup(x => x.CreateStepAsync(It.IsAny<CreateStepDto>(), It.IsAny<CancellationToken>()))
            .Returns<CreateStepDto, CancellationToken>((dto, ct) =>
                Task.FromResult(new StepDto
                {
                    Id = Guid.NewGuid(),
                    SubscriptionHistoryId = dto.SubscriptionHistoryId,
                    Status = dto.Status,
                    Message = dto.Message,
                    CreatedAt = DateTimeOffset.UtcNow
                }));

        // Настройка PublishEndpoint
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new SubscriptionService(
            _subscriptionRepositoryMock.Object,
            _historyClientMock.Object,
            _contractRepositoryMock.Object,
            _cacheMock.Object,
            _publishEndpointMock.Object);
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
        var contract = CreateContract();

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _subscriptionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.ContractId.ShouldBe(createDto.ContractId);

        _subscriptionRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.CreateHistoryAsync(It.IsAny<SubscriptionHistoryEventDto>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.UpdateHistoryStatusAsync(It.IsAny<Guid>(), Domain.Enums.SubscriptionActionStatus.Pending, It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.CreateStepAsync(It.IsAny<CreateStepDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatesHistoryWithOpenActionAndPendingStatus()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();
        var contract = CreateContract();

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _subscriptionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();

        _historyClientMock.Verify(
            x => x.CreateHistoryAsync(
                It.Is<SubscriptionHistoryEventDto>(h =>
                    h.ActionType == Domain.Enums.SubscriptionActionType.Open &&
                    h.Status == Domain.Enums.SubscriptionActionStatus.Pending),
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

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<SubscriptionUpdatedEvent>(), It.IsAny<CancellationToken>()),
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

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<SubscriptionDeletedEvent>(), It.IsAny<CancellationToken>()),
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
        var subscriptions = CreateSubscriptionEntityList(3);
        foreach (var subscription in subscriptions)
        {
            subscription.ContractId = contractId;
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
        var activeSubscription = CreateSubscriptionEntity(Guid.NewGuid());
        activeSubscription.ContractId = contractId;
        activeSubscription.EndDate = null;

        var expiredSubscription = CreateSubscriptionEntity(Guid.NewGuid());
        expiredSubscription.ContractId = contractId;
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
        result.ShouldBeOfType<SubscriptionFullDataDto>();

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
        var newTariffPlanId = Guid.NewGuid();

        var contract = CreateContract();
        contract.Id = existingSubscription.ContractId;

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
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

        _historyClientMock.Verify(
            x => x.CreateHistoryAsync(
                It.Is<SubscriptionHistoryEventDto>(h =>
                    h.ActionType == Domain.Enums.SubscriptionActionType.TariffChange &&
                    h.Status == Domain.Enums.SubscriptionActionStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.UpdateHistoryStatusAsync(It.IsAny<Guid>(), Domain.Enums.SubscriptionActionStatus.Pending, It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.CreateStepAsync(It.IsAny<CreateStepDto>(), It.IsAny<CancellationToken>()),
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

        var contract = CreateContract();
        contract.Id = existingSubscription.ContractId;

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _subscriptionRepositoryMock
            .Setup(r => r.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        _subscriptionRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
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

        _historyClientMock.Verify(
            x => x.CreateHistoryAsync(
                It.Is<SubscriptionHistoryEventDto>(h =>
                    h.ActionType == Domain.Enums.SubscriptionActionType.Close &&
                    h.Status == Domain.Enums.SubscriptionActionStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.UpdateHistoryStatusAsync(It.IsAny<Guid>(), Domain.Enums.SubscriptionActionStatus.Pending, It.IsAny<CancellationToken>()),
            Times.Once);

        _historyClientMock.Verify(
            x => x.CreateStepAsync(It.IsAny<CreateStepDto>(), It.IsAny<CancellationToken>()),
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
        var subscriptions = CreateSubscriptionEntityList(3);
        foreach (var sub in subscriptions)
        {
            sub.Contract = CreateContract(abonentId);
            sub.ContractId = sub.Contract.Id;
        }

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
        var activeSubscription = CreateSubscriptionEntity(Guid.NewGuid());
        activeSubscription.Contract = CreateContract(abonentId);
        activeSubscription.ContractId = activeSubscription.Contract.Id;
        activeSubscription.EndDate = null;

        var expiredSubscription = CreateSubscriptionEntity(Guid.NewGuid());
        expiredSubscription.Contract = CreateContract(abonentId);
        expiredSubscription.ContractId = expiredSubscription.Contract.Id;
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
        var activeSubscription = CreateSubscriptionEntity(Guid.NewGuid());
        activeSubscription.Contract = CreateContract(abonentId);
        activeSubscription.ContractId = activeSubscription.Contract.Id;
        activeSubscription.EndDate = null;

        var expiredSubscription = CreateSubscriptionEntity(Guid.NewGuid());
        expiredSubscription.Contract = CreateContract(abonentId);
        expiredSubscription.ContractId = expiredSubscription.Contract.Id;
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
        return new AutoFaker<SubscriptionDto>()
            .RuleFor(d => d.ContractId, _ => Guid.NewGuid())
            .RuleFor(d => d.ServiceId, _ => Guid.NewGuid())
            .RuleFor(d => d.TariffPlanId, _ => Guid.NewGuid())
            .Generate();
    }

    private static UpdateSubscriptionDto CreateUpdateSubscriptionDto()
    {
        return new AutoFaker<UpdateSubscriptionDto>()
            .RuleFor(d => d.TariffPlanId, _ => Guid.NewGuid())
            .RuleFor(d => d.EndDate, _ => DateTimeOffset.UtcNow.AddDays(30))
            .Generate();
    }

    #endregion
}