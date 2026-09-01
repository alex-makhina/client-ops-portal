using AutoBogus;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Exceptions;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Data;
using ClientOpsPortal.Services.SubscriptionHistory.Services;
using Moq;
using Shouldly;
using System.Linq.Expressions;
using Xunit;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Services;

public class SubscriptionHistoryServiceTests
{
    private readonly Mock<IMongoRepository<SubscriptionHistoryModel>> _historyRepositoryMock;
    private readonly Mock<IMongoRepository<SubscriptionHistoryStep>> _stepRepositoryMock;
    private readonly SubscriptionHistoryService _sut;

    public SubscriptionHistoryServiceTests()
    {
        _historyRepositoryMock = new Mock<IMongoRepository<SubscriptionHistoryModel>>();
        _stepRepositoryMock = new Mock<IMongoRepository<SubscriptionHistoryStep>>();
        _sut = new SubscriptionHistoryService(
            _historyRepositoryMock.Object,
            _stepRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSubscriptionHistoryByIdAsync_WhenHistoryExists_ReturnsDto()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = CreateSubscriptionHistoryEntity(historyId);

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _sut.GetSubscriptionHistoryByIdAsync(historyId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(historyId);
        result.SubscriptionId.ShouldBe(history.SubscriptionId);

        _historyRepositoryMock.Verify(
            r => r.GetByIdAsync(historyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionHistoryByIdAsync_WhenHistoryNotFound_ReturnsNull()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryModel?)null);

        // Act
        var result = await _sut.GetSubscriptionHistoryByIdAsync(historyId);

        // Assert
        result.ShouldBeNull();

        _historyRepositoryMock.Verify(
            r => r.GetByIdAsync(historyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllSubscriptionHistoryAsync_WhenHistoriesExist_ReturnsList()
    {
        // Arrange
        var histories = CreateSubscriptionHistoryEntityList(5);

        _historyRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _sut.GetAllSubscriptionHistoryAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(5);

        _historyRepositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionHistoryAsync_WhenValidDto_ReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryDto();

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistoryModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateSubscriptionHistoryAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.SubscriptionId.ShouldBe(createDto.SubscriptionId);
        result.ActionType.ShouldBe(createDto.ActionType);

        _historyRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistoryModel>(h =>
                    h.SubscriptionId == createDto.SubscriptionId &&
                    h.ActionType == createDto.ActionType &&
                    h.Status == createDto.Status),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSubscriptionHistoryAsync_WhenHistoryExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var existingHistory = CreateSubscriptionHistoryEntity(historyId);
        var updateDto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHistory);

        _historyRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistoryModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateSubscriptionHistoryAsync(historyId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(historyId);
        result.Status.ShouldBe(SubscriptionActionStatus.Completed);

        _historyRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistoryModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSubscriptionHistoryAsync_WhenHistoryNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var updateDto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryModel?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateSubscriptionHistoryAsync(historyId, updateDto));

        _historyRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistoryModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteSubscriptionHistoryAsync_WhenHistoryExists_CallsRepositoryDelete()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyRepositoryMock
            .Setup(r => r.DeleteAsync(historyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteSubscriptionHistoryAsync(historyId);

        // Assert
        _historyRepositoryMock.Verify(
            r => r.DeleteAsync(historyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionHistoryWhereAsync_WithPredicate_ReturnsFilteredHistories()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var histories = CreateSubscriptionHistoryEntityList(3);
        foreach (var history in histories)
        {
            history.SubscriptionId = subscriptionId;
        }

        _historyRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryModel, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _sut.GetSubscriptionHistoryWhereAsync(h => h.SubscriptionId == subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        _historyRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<SubscriptionHistoryModel, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoryBySubscriptionAsync_WhenHistoriesExist_ReturnsOrderedDtos()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var histories = CreateSubscriptionHistoryEntityList(3);

        // Устанавливаем разные даты для проверки сортировки
        for (int i = 0; i < histories.Count; i++)
        {
            histories[i].SubscriptionId = subscriptionId;
            histories[i].CreatedAt = DateTimeOffset.UtcNow.AddMinutes(i * 10);
        }

        _historyRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryModel, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _sut.GetHistoryBySubscriptionAsync(subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        // Используем ToList() для доступа по индексу
        var resultList = result.ToList();
        for (int i = 0; i < resultList.Count - 1; i++)
        {
            resultList[i].CreatedAt.ShouldBeGreaterThanOrEqualTo(resultList[i + 1].CreatedAt);
        }

        _historyRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.Is<Expression<Func<SubscriptionHistoryModel, bool>>>(expr => expr.ToString().Contains("SubscriptionId")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionsHistoryByAbonentIdAsync_WhenHistoriesExist_ReturnsFullDtos()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var histories = CreateSubscriptionHistoryEntityList(2);
        foreach (var history in histories)
        {
            history.AbonentId = abonentId;
            history.Steps = new List<SubscriptionHistoryStep>
            {
                new AutoFaker<SubscriptionHistoryStep>().Generate(),
                new AutoFaker<SubscriptionHistoryStep>().Generate()
            };
        }

        _historyRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryModel, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        _stepRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionHistoryStep>());

        // Act
        var result = await _sut.GetSubscriptionsHistoryByAbonentIdAsync(abonentId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        foreach (var dto in result)
        {
            dto.AbonentId.ShouldBe(abonentId);
            dto.Steps.ShouldNotBeNull();
        }

        _historyRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.Is<Expression<Func<SubscriptionHistoryModel, bool>>>(expr => expr.ToString().Contains("AbonentId")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SubscriptionHistoryModel CreateSubscriptionHistoryEntity(Guid? id = null)
    {
        var faker = new AutoFaker<SubscriptionHistoryModel>();

        if (id.HasValue)
            faker.RuleFor(h => h.Id, _ => id.Value);

        return faker
            .RuleFor(h => h.Steps, _ => new List<SubscriptionHistoryStep>())
            .Generate();
    }

    private static List<SubscriptionHistoryModel> CreateSubscriptionHistoryEntityList(int count)
    {
        var list = new List<SubscriptionHistoryModel>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionHistoryEntity());
        }
        return list;
    }

    private static CreateSubscriptionHistoryDto CreateCreateSubscriptionHistoryDto()
    {
        return new AutoFaker<CreateSubscriptionHistoryDto>()
            .RuleFor(dto => dto.Steps, _ => new List<SubscriptionHistoryStep>())
            .Generate();
    }
}