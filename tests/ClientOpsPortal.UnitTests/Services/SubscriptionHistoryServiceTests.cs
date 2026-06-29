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

public class SubscriptionHistoryServiceTests
{
    private readonly Mock<IGenericRepository<SubscriptionHistory>> _historyRepositoryMock;
    private readonly Mock<IGenericRepository<SubscriptionHistoryStep>> _stepRepositoryMock;
    private readonly Mock<IGenericRepository<Subscription>> _subscriptionRepositoryMock;
    private readonly SubscriptionHistoryService _sut;

    public SubscriptionHistoryServiceTests()
    {
        _historyRepositoryMock = new Mock<IGenericRepository<SubscriptionHistory>>();
        _stepRepositoryMock = new Mock<IGenericRepository<SubscriptionHistoryStep>>();
        _subscriptionRepositoryMock = new Mock<IGenericRepository<Subscription>>();
        _sut = new SubscriptionHistoryService(
            _historyRepositoryMock.Object,
            _stepRepositoryMock.Object,
            _subscriptionRepositoryMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenHistoryExists_ReturnsHistoryDto()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = CreateSubscriptionHistoryEntity(historyId);

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _sut.GetByIdAsync(historyId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(historyId);
        result.SubscriptionId.ShouldBe(history.SubscriptionId);
        result.ActionType.ShouldBe(history.ActionType);

        _historyRepositoryMock.Verify(
            r => r.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenHistoryNotFound_ReturnsNull()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistory?)null);

        // Act
        var result = await _sut.GetByIdAsync(historyId, true);

        // Assert
        result.ShouldBeNull();

        _historyRepositoryMock.Verify(
            r => r.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludesFalse_PassesParameterToRepository()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var history = CreateSubscriptionHistoryEntity(historyId);

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _sut.GetByIdAsync(historyId, false);

        // Assert
        result.ShouldNotBeNull();

        _historyRepositoryMock.Verify(
            r => r.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenHistoriesExist_ReturnsListOfHistoryDtos()
    {
        // Arrange
        var histories = CreateSubscriptionHistoryEntityList(5);
        var expectedCount = histories.Count;

        _historyRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _historyRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoHistoriesExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionHistory>();

        _historyRepositoryMock
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
    public async Task CreateAsync_WhenValidDto_CreatesHistoryAndReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryDto();
        var createdHistory = createDto.ToEntity();

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Callback<SubscriptionHistory, CancellationToken>((h, ct) =>
            {
                h.Id = createdHistory.Id;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.SubscriptionId.ShouldBe(createDto.SubscriptionId);
        result.ActionType.ShouldBe(createDto.ActionType);
        result.Status.ShouldBe(createDto.Status);
        result.TariffPlanId.ShouldBe(createDto.TariffPlanId);

        _historyRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistory>(h =>
                    h.SubscriptionId == createDto.SubscriptionId &&
                    h.ActionType == createDto.ActionType &&
                    h.Status == createDto.Status),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithSteps_CreatesHistoryWithSteps()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryDto();
        createDto.Steps = new List<SubscriptionHistoryStep>
    {
        new SubscriptionHistoryStep { Status = SubscriptionActionStatus.Pending, Message = "В ожидании отправки на активацию" },
        new SubscriptionHistoryStep { Status = SubscriptionActionStatus.InProgress, Message = "В обработке" },
        new SubscriptionHistoryStep { Status = SubscriptionActionStatus.Completed, Message = "Выполнен" }
    };

        _historyRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Steps.Count.ShouldBe(3);

        _historyRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistory>(h => h.Steps.Count == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesHistoryAndReturnsDto()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var existingHistory = CreateSubscriptionHistoryEntity(historyId);
        var updateDto = CreateUpdateSubscriptionHistoryDto();

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHistory);

        _historyRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(historyId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(historyId);

        _historyRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenHistoryNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryDto();

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistory?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(historyId, updateDto));

        _historyRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesStatusCorrectly()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var existingHistory = CreateSubscriptionHistoryEntity(historyId);
        var updateDto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        _historyRepositoryMock
            .Setup(r => r.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHistory);

        _historyRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(historyId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _historyRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<SubscriptionHistory>(h => h.Status == SubscriptionActionStatus.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenHistoryExists_CallsRepositoryDelete()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyRepositoryMock
            .Setup(r => r.DeleteAsync(historyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(historyId);

        // Assert
        _historyRepositoryMock.Verify(
            r => r.DeleteAsync(historyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredHistories()
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
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _sut.GetWhereAsync(h => h.SubscriptionId == subscriptionId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(histories.Count);

        _historyRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionHistory>();

        _historyRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetWhereAsync(h => h.SubscriptionId == Guid.NewGuid(), false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetHistoryBySubscriptionAsync Tests

    [Fact]
    public async Task GetHistoryBySubscriptionAsync_WhenHistoriesExist_ReturnsHistoryDtos()
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
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _sut.GetHistoryBySubscriptionAsync(subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(histories.Count);

        foreach (var historyDto in result)
        {
            historyDto.SubscriptionId.ShouldBe(subscriptionId);
        }

        _historyRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoryBySubscriptionAsync_WhenNoHistoriesExist_ReturnsEmptyList()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var emptyList = new List<SubscriptionHistory>();

        _historyRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetHistoryBySubscriptionAsync(subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetHistoryBySubscriptionAsync_ReturnsHistoriesWithSteps()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var history = CreateSubscriptionHistoryEntity(Guid.NewGuid());
        history.SubscriptionId = subscriptionId;
        history.Steps = new List<SubscriptionHistoryStep>
    {
        new SubscriptionHistoryStep { Status = SubscriptionActionStatus.Pending, Message = "В ожидании отправки на активацию" },
        new SubscriptionHistoryStep { Status = SubscriptionActionStatus.InProgress, Message = "В обработке" },
        new SubscriptionHistoryStep { Status = SubscriptionActionStatus.Completed, Message = "Выполнен" }
    };

        _historyRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionHistory> { history });

        // Act
        var result = await _sut.GetHistoryBySubscriptionAsync(subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var historyDto = result.First();
        historyDto.Steps.ShouldNotBeNull();
        historyDto.Steps.Count.ShouldBe(3);

        historyDto.Steps[0].Status.ShouldBe(SubscriptionActionStatus.Pending);
        historyDto.Steps[0].Message.ShouldBe("В ожидании отправки на активацию");

        historyDto.Steps[1].Status.ShouldBe(SubscriptionActionStatus.InProgress);
        historyDto.Steps[1].Message.ShouldBe("В обработке");

        historyDto.Steps[2].Status.ShouldBe(SubscriptionActionStatus.Completed);
        historyDto.Steps[2].Message.ShouldBe("Выполнен");
    }

    #endregion

    #region Helper Methods

    private static SubscriptionHistory CreateSubscriptionHistoryEntity(Guid? id = null)
    {
        var faker = new AutoFaker<SubscriptionHistory>();

        if (id.HasValue)
            faker.RuleFor(h => h.Id, _ => id.Value);

        return faker
            .RuleFor(h => h.Steps, _ => new List<SubscriptionHistoryStep>())
            .Generate();
    }

    private static List<SubscriptionHistory> CreateSubscriptionHistoryEntityList(int count)
    {
        var list = new List<SubscriptionHistory>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionHistoryEntity());
        }
        return list;
    }

    private static CreateSubscriptionHistoryDto CreateCreateSubscriptionHistoryDto()
    {
        var faker = new AutoFaker<CreateSubscriptionHistoryDto>();
        return faker
            .RuleFor(dto => dto.Steps, _ => new List<SubscriptionHistoryStep>())
            .Generate();
    }

    private static UpdateSubscriptionHistoryDto CreateUpdateSubscriptionHistoryDto()
    {
        return new AutoFaker<UpdateSubscriptionHistoryDto>().Generate();
    }

    #endregion
}