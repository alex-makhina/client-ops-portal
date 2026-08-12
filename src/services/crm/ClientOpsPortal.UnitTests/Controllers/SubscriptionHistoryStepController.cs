using AutoBogus;
using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;

namespace ClientOpsPortal.UnitTests.Controllers;

public class SubscriptionHistoryStepsControllerTests
{
    private readonly Mock<ISubscriptionHistoryStepService> _stepServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly SubscriptionHistoryStepsController _sut;

    public SubscriptionHistoryStepsControllerTests()
    {
        _stepServiceMock = new Mock<ISubscriptionHistoryStepService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new SubscriptionHistoryStepsController(
            _stepServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenStepsExist_ReturnsOkWithSteps()
    {
        // Arrange
        var expectedSteps = CreateSubscriptionHistoryStepDtoList(5);

        _stepServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSteps);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var steps = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryStepDto>>();
        steps.Count().ShouldBe(5);

        _stepServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoStepsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionHistoryStepDto>().AsReadOnly();

        _stepServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var steps = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryStepDto>>();
        steps.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedSteps = CreateSubscriptionHistoryStepDtoList(3);

        _stepServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSteps);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _stepServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenStepExists_ReturnsOkWithStep()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var expectedStep = CreateSubscriptionHistoryStepDto(stepId);

        _stepServiceMock
            .Setup(s => s.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStep);

        // Act
        var result = await _sut.GetByIdAsync(stepId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var step = okResult.Value.ShouldBeOfType<SubscriptionHistoryStepDto>();

        step.Id.ShouldBe(stepId);

        _stepServiceMock.Verify(
            s => s.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenStepNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepServiceMock
            .Setup(s => s.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryStepDto?)null);

        // Act
        var result = await _sut.GetByIdAsync(stepId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(stepId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _stepServiceMock.Verify(
            s => s.GetByIdAsync(stepId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByHistory Tests

    [Fact]
    public async Task GetByHistory_WhenStepsExist_ReturnsOkWithSteps()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var expectedSteps = CreateSubscriptionHistoryStepDtoList(4);

        _stepServiceMock
            .Setup(s => s.GetStepsByHistoryAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSteps);

        // Act
        var result = await _sut.GetByHistory(historyId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var steps = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryStepDto>>();
        steps.Count().ShouldBe(4);

        _stepServiceMock.Verify(
            s => s.GetStepsByHistoryAsync(historyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByHistory_WhenNoStepsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var emptyList = new List<SubscriptionHistoryStepDto>().AsReadOnly();

        _stepServiceMock
            .Setup(s => s.GetStepsByHistoryAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetByHistory(historyId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var steps = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryStepDto>>();
        steps.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithStep()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryStepDto();
        var createdDto = CreateSubscriptionHistoryStepDto(id: Guid.NewGuid(), historyId: createDto.SubscriptionHistoryId);

        _stepServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(SubscriptionHistoryStepsController.GetByIdAsync));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<SubscriptionHistoryStepDto>().ShouldBe(createdDto);

        _stepServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenHistoryNotFound_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryStepDto();
        var errorMessage = $"SubscriptionHistory with Id '{createDto.SubscriptionHistoryId}' was not found.";

        _stepServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(SubscriptionHistory), createDto.SubscriptionHistoryId));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _stepServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenStepExists_ReturnsOkWithUpdatedStep()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryStepDto();
        var updatedDto = CreateSubscriptionHistoryStepDto(stepId);

        _stepServiceMock
            .Setup(s => s.UpdateAsync(stepId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(stepId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var step = okResult.Value.ShouldBeOfType<SubscriptionHistoryStepDto>();

        step.Id.ShouldBe(stepId);

        _stepServiceMock.Verify(
            s => s.UpdateAsync(stepId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenStepNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryStepDto();

        _stepServiceMock
            .Setup(s => s.UpdateAsync(stepId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(SubscriptionHistoryStep), stepId));

        // Act
        var result = await _sut.Update(stepId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(stepId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    [Fact]
    public async Task Update_WhenStepReturnsNull_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryStepDto();

        _stepServiceMock
            .Setup(s => s.UpdateAsync(stepId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryStepDto?)null);

        // Act
        var result = await _sut.Update(stepId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(stepId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenStepExists_ReturnsNoContent()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepServiceMock
            .Setup(s => s.DeleteAsync(stepId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(stepId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _stepServiceMock.Verify(
            s => s.DeleteAsync(stepId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenStepNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        _stepServiceMock
            .Setup(s => s.DeleteAsync(stepId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(SubscriptionHistoryStep), stepId));

        // Act
        var result = await _sut.Delete(stepId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(stepId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Helper Methods

    private static CreateSubscriptionHistoryStepDto CreateCreateSubscriptionHistoryStepDto()
    {
        return new AutoFaker<CreateSubscriptionHistoryStepDto>().Generate();
    }

    private static UpdateSubscriptionHistoryStepDto CreateUpdateSubscriptionHistoryStepDto()
    {
        return new AutoFaker<UpdateSubscriptionHistoryStepDto>().Generate();
    }

    private static SubscriptionHistoryStepDto CreateSubscriptionHistoryStepDto(Guid? id = null, Guid? historyId = null)
    {
        var autoFaker = new AutoFaker<SubscriptionHistoryStepDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);

        if (historyId.HasValue)
            autoFaker.RuleFor(dto => dto.SubscriptionHistoryId, _ => historyId.Value);

        return autoFaker.Generate();
    }

    private static IReadOnlyCollection<SubscriptionHistoryStepDto> CreateSubscriptionHistoryStepDtoList(int count)
    {
        var list = new List<SubscriptionHistoryStepDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionHistoryStepDto());
        }
        return list.AsReadOnly();
    }

    #endregion
}