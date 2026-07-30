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

public class SubscriptionHistoryStepServiceTests
{
    private readonly Mock<IGenericRepository<SubscriptionHistoryStep>> _stepRepositoryMock;
    private readonly Mock<IGenericRepository<SubscriptionHistory>> _historyRepositoryMock;
    private readonly SubscriptionHistoryStepService _sut;

    public SubscriptionHistoryStepServiceTests()
    {
        _stepRepositoryMock = new Mock<IGenericRepository<SubscriptionHistoryStep>>();
        _historyRepositoryMock = new Mock<IGenericRepository<SubscriptionHistory>>();
        _sut = new SubscriptionHistoryStepService(
            _stepRepositoryMock.Object,
            _historyRepositoryMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenStepExists_ReturnsStepDto()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var step = CreateSubscriptionHistoryStepEntity(stepId);

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(step);

        // Act
        var result = await _sut.GetByIdAsync(stepId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(stepId);
        result.SubscriptionHistoryId.ShouldBe(step.SubscriptionHistoryId);
        result.Status.ShouldBe(step.Status);

        _stepRepositoryMock.Verify(
            r => r.GetByIdAsync(stepId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenStepNotFound_ReturnsNull()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryStep?)null);

        // Act
        var result = await _sut.GetByIdAsync(stepId, true);

        // Assert
        result.ShouldBeNull();

        _stepRepositoryMock.Verify(
            r => r.GetByIdAsync(stepId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludesFalse_PassesParameterToRepository()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var step = CreateSubscriptionHistoryStepEntity(stepId);

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(step);

        // Act
        var result = await _sut.GetByIdAsync(stepId, false);

        // Assert
        result.ShouldNotBeNull();

        _stepRepositoryMock.Verify(
            r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenStepsExist_ReturnsListOfStepDtos()
    {
        // Arrange
        var steps = CreateSubscriptionHistoryStepEntityList(5);
        var expectedCount = steps.Count;

        _stepRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _stepRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoStepsExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionHistoryStep>();

        _stepRepositoryMock
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAllAsync(false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredSteps()
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
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        // Act
        var result = await _sut.GetWhereAsync(s => s.SubscriptionHistoryId == historyId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(steps.Count);

        _stepRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionHistoryStep>();

        _stepRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetWhereAsync(s => s.SubscriptionHistoryId == Guid.NewGuid(), false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidDto_CreatesStepAndReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryStepDto();
        var createdStep = createDto.ToEntity();

        _stepRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Callback<SubscriptionHistoryStep, CancellationToken>((s, ct) =>
            {
                s.Id = createdStep.Id;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.SubscriptionHistoryId.ShouldBe(createDto.SubscriptionHistoryId);
        result.Status.ShouldBe(createDto.Status);
        result.Message.ShouldBe(createDto.Message);

        _stepRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistoryStep>(s =>
                    s.SubscriptionHistoryId == createDto.SubscriptionHistoryId &&
                    s.Status == createDto.Status &&
                    s.Message == createDto.Message),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithoutMessage_CreatesStepWithNullMessage()
    {
        // Arrange
        var createDto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = null
        };

        _stepRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Message.ShouldBeNull();

        _stepRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<SubscriptionHistoryStep>(s => s.Message == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesStepAndReturnsDto()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var existingStep = CreateSubscriptionHistoryStepEntity(stepId);
        var updateDto = CreateUpdateSubscriptionHistoryStepDto();

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStep);

        _stepRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(stepId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(stepId);

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenStepNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryStepDto();

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryStep?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(stepId, updateDto));

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesStatusCorrectly()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var existingStep = CreateSubscriptionHistoryStepEntity(stepId);
        existingStep.Status = SubscriptionActionStatus.Pending;
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed,
            Message = existingStep.Message
        };

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStep);

        _stepRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(stepId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<SubscriptionHistoryStep>(s => s.Status == SubscriptionActionStatus.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMessageCorrectly()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var existingStep = CreateSubscriptionHistoryStepEntity(stepId);
        var newMessage = "Обновленное сообщение";
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = null,
            Message = newMessage
        };

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStep);

        _stepRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(stepId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<SubscriptionHistoryStep>(s => s.Message == newMessage),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBothStatusAndMessageCorrectly()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var existingStep = CreateSubscriptionHistoryStepEntity(stepId);
        var newMessage = "Обновленное сообщение";
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Failed,
            Message = newMessage
        };

        _stepRepositoryMock
            .Setup(r => r.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStep);

        _stepRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SubscriptionHistoryStep>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(stepId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _stepRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<SubscriptionHistoryStep>(s =>
                    s.Status == SubscriptionActionStatus.Failed &&
                    s.Message == newMessage),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenStepExists_CallsRepositoryDelete()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepRepositoryMock
            .Setup(r => r.DeleteAsync(stepId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(stepId);

        // Assert
        _stepRepositoryMock.Verify(
            r => r.DeleteAsync(stepId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetStepsByHistoryAsync Tests

    [Fact]
    public async Task GetStepsByHistoryAsync_WhenStepsExist_ReturnsStepDtos()
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
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        // Act
        var result = await _sut.GetStepsByHistoryAsync(historyId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(steps.Count);

        foreach (var stepDto in result)
        {
            stepDto.SubscriptionHistoryId.ShouldBe(historyId);
        }

        _stepRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStepsByHistoryAsync_WhenNoStepsExist_ReturnsEmptyList()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var emptyList = new List<SubscriptionHistoryStep>();

        _stepRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistoryStep, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetStepsByHistoryAsync(historyId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region Helper Methods

    private static SubscriptionHistoryStep CreateSubscriptionHistoryStepEntity(
        Guid? id = null,
        Guid? historyId = null,
        SubscriptionActionStatus? status = null,
        string? message = null)
    {
        var faker = new AutoFaker<SubscriptionHistoryStep>();

        if (id.HasValue)
            faker.RuleFor(s => s.Id, _ => id.Value);

        if (historyId.HasValue)
            faker.RuleFor(s => s.SubscriptionHistoryId, _ => historyId.Value);

        if (status.HasValue)
            faker.RuleFor(s => s.Status, _ => status.Value);

        if (message != null)
            faker.RuleFor(s => s.Message, _ => message);

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

    private static UpdateSubscriptionHistoryStepDto CreateUpdateSubscriptionHistoryStepDto()
    {
        return new AutoFaker<UpdateSubscriptionHistoryStepDto>().Generate();
    }

    #endregion
}