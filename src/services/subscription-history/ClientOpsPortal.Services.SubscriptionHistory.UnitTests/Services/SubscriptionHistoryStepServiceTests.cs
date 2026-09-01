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

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Services;

public class SubscriptionHistoryStepServiceTests
{
    private readonly Mock<IMongoRepository<SubscriptionHistoryStep>> _stepRepositoryMock;
    private readonly SubscriptionHistoryStepService _sut;

    public SubscriptionHistoryStepServiceTests()
    {
        _stepRepositoryMock = new Mock<IMongoRepository<SubscriptionHistoryStep>>();
        _sut = new SubscriptionHistoryStepService(_stepRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSubscriptionHistoryStepByIdAsync_WhenStepExists_ReturnsDto()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var step = CreateSubscriptionHistoryStepEntity(stepId);

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(step);

        // Act
        var result = await _sut.GetSubscriptionHistoryStepByIdAsync(stepId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(stepId);
        result.SubscriptionHistoryId.ShouldBe(step.SubscriptionHistoryId);

        _stepRepositoryMock.Verify(
            r => r.GetByIdAsync(stepId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionHistoryStepByIdAsync_WhenStepNotFound_ReturnsNull()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryStep?)null);

        // Act
        var result = await _sut.GetSubscriptionHistoryStepByIdAsync(stepId);

        // Assert
        result.ShouldBeNull();

        _stepRepositoryMock.Verify(
            r => r.GetByIdAsync(stepId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllSubscriptionHistoryStepAsync_WhenStepsExist_ReturnsList()
    {
        // Arrange
        var steps = CreateSubscriptionHistoryStepEntityList(5);

        _stepRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        // Act
        var result = await _sut.GetAllSubscriptionHistoryStepAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(5);

        _stepRepositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionHistoryStepAsync_WhenValidDto_ReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryStepDto();

        _stepRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateSubscriptionHistoryStepAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.SubscriptionHistoryId.ShouldBe(createDto.SubscriptionHistoryId);
        result.Status.ShouldBe(createDto.Status);

        _stepRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistoryStep>(s =>
                    s.SubscriptionHistoryId == createDto.SubscriptionHistoryId &&
                    s.Status == createDto.Status),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSubscriptionHistoryStepAsync_WhenStepExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var existingStep = CreateSubscriptionHistoryStepEntity(stepId);
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed,
            Message = "Updated message"
        };

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStep);

        _stepRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateSubscriptionHistoryStepAsync(stepId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(stepId);
        result.Status.ShouldBe(SubscriptionActionStatus.Completed);
        result.Message.ShouldBe("Updated message");

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSubscriptionHistoryStepAsync_WhenStepNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryStep?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateSubscriptionHistoryStepAsync(stepId, updateDto));

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteSubscriptionHistoryStepAsync_WhenStepExists_CallsRepositoryDelete()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepRepositoryMock
            .Setup(r => r.DeleteAsync(stepId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteSubscriptionHistoryStepAsync(stepId);

        // Assert
        _stepRepositoryMock.Verify(
            r => r.DeleteAsync(stepId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSubscriptionHistoryStepWhereAsync_WithPredicate_ReturnsFilteredSteps()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var steps = CreateSubscriptionHistoryStepEntityList(3);
        foreach (var step in steps)
        {
            step.SubscriptionHistoryId = historyId;
        }

        _stepRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        // Act
        var result = await _sut.GetSubscriptionHistoryStepWhereAsync(s => s.SubscriptionHistoryId == historyId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        _stepRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStepsByHistoryAsync_WhenStepsExist_ReturnsOrderedDtos()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var steps = CreateSubscriptionHistoryStepEntityList(3);
        foreach (var step in steps)
        {
            step.SubscriptionHistoryId = historyId;
        }

        _stepRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        // Act
        var result = await _sut.GetStepsByHistoryAsync(historyId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        var resultList = result.ToList();
        for (int i = 0; i < resultList.Count - 1; i++)
        {
            resultList[i].CreatedAt.ShouldBeLessThanOrEqualTo(resultList[i + 1].CreatedAt);
        }

        _stepRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.Is<Expression<Func<SubscriptionHistoryStep, bool>>>(expr => expr.ToString().Contains("SubscriptionHistoryId")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SubscriptionHistoryStep CreateSubscriptionHistoryStepEntity(Guid? id = null)
    {
        var faker = new AutoFaker<SubscriptionHistoryStep>();

        if (id.HasValue)
            faker.RuleFor(s => s.Id, _ => id.Value);

        return faker.Generate();
    }

    private static List<SubscriptionHistoryStep> CreateSubscriptionHistoryStepEntityList(int count)
    {
        var list = new List<SubscriptionHistoryStep>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionHistoryStepEntity());
        }
        return list;
    }

    private static CreateSubscriptionHistoryStepDto CreateCreateSubscriptionHistoryStepDto()
    {
        return new AutoFaker<CreateSubscriptionHistoryStepDto>().Generate();
    }
}