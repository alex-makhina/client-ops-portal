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
using System.Linq.Expressions;
using System.Security.Claims;

namespace ClientOpsPortal.UnitTests.Controllers;

public class SubscriptionHistoriesControllerTests
{
    private readonly Mock<ISubscriptionHistoryService> _historyServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly SubscriptionHistoriesController _sut;

    public SubscriptionHistoriesControllerTests()
    {
        _historyServiceMock = new Mock<ISubscriptionHistoryService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new SubscriptionHistoriesController(
            _historyServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenHistoriesExist_ReturnsOkWithHistories()
    {
        // Arrange
        var expectedHistories = CreateSubscriptionHistoryDtoList(5);

        _historyServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistories);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var histories = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryDto>>();
        histories.Count().ShouldBe(5);

        _historyServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoHistoriesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionHistoryDto>().AsReadOnly();

        _historyServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var histories = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryDto>>();
        histories.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedHistories = CreateSubscriptionHistoryDtoList(3);

        _historyServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistories);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _historyServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenHistoryExists_ReturnsOkWithHistory()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var expectedHistory = CreateSubscriptionHistoryDto(historyId);

        _historyServiceMock
            .Setup(s => s.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _sut.GetById(historyId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var history = okResult.Value.ShouldBeOfType<SubscriptionHistoryDto>();

        history.Id.ShouldBe(historyId);

        _historyServiceMock.Verify(
            s => s.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenHistoryNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyServiceMock
            .Setup(s => s.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionHistoryDto?)null);

        // Act
        var result = await _sut.GetById(historyId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(historyId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _historyServiceMock.Verify(
            s => s.GetByIdAsync(historyId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToService()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var expectedHistory = CreateSubscriptionHistoryDto(historyId);

        _historyServiceMock
            .Setup(s => s.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _sut.GetById(historyId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _historyServiceMock.Verify(
            s => s.GetByIdAsync(historyId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetBySubscription Tests

    [Fact]
    public async Task GetBySubscription_WhenHistoriesExist_ReturnsOkWithHistories()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var expectedHistories = CreateSubscriptionHistoryDtoList(3);

        _historyServiceMock
            .Setup(s => s.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistories);

        // Act
        var result = await _sut.GetBySubscription(subscriptionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var histories = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryDto>>();
        histories.Count().ShouldBe(3);

        _historyServiceMock.Verify(
            s => s.GetWhereAsync(
                It.Is<Expression<Func<SubscriptionHistory, bool>>>(expr => expr.ToString().Contains("SubscriptionId")),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBySubscription_WhenNoHistoriesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var emptyList = new List<SubscriptionHistoryDto>().AsReadOnly();

        _historyServiceMock
            .Setup(s => s.GetWhereAsync(
                It.IsAny<Expression<Func<SubscriptionHistory, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetBySubscription(subscriptionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var histories = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionHistoryDto>>();
        histories.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithHistory()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryDto();
        var createdDto = CreateSubscriptionHistoryDto(id: Guid.NewGuid(), subscriptionId: createDto.SubscriptionId);

        _historyServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(SubscriptionHistoriesController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<SubscriptionHistoryDto>().ShouldBe(createdDto);

        _historyServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryDto();
        var errorMessage = "Ошибка создания истории подписки";

        _historyServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _historyServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenHistoryExists_ReturnsOkWithUpdatedHistory()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryDto();
        var updatedDto = CreateSubscriptionHistoryDto(historyId);

        _historyServiceMock
            .Setup(s => s.UpdateAsync(historyId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(historyId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var history = okResult.Value.ShouldBeOfType<SubscriptionHistoryDto>();

        history.Id.ShouldBe(historyId);

        _historyServiceMock.Verify(
            s => s.UpdateAsync(historyId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenHistoryNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionHistoryDto();

        _historyServiceMock
            .Setup(s => s.UpdateAsync(historyId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(SubscriptionHistory), historyId));

        // Act
        var result = await _sut.Update(historyId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(historyId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenHistoryExists_ReturnsNoContent()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyServiceMock
            .Setup(s => s.DeleteAsync(historyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(historyId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _historyServiceMock.Verify(
            s => s.DeleteAsync(historyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenHistoryNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var historyId = Guid.NewGuid();

        _historyServiceMock
            .Setup(s => s.DeleteAsync(historyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(SubscriptionHistory), historyId));

        // Act
        var result = await _sut.Delete(historyId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(historyId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region Helper Methods

    private static CreateSubscriptionHistoryDto CreateCreateSubscriptionHistoryDto()
    {
        var autoFaker = new AutoFaker<CreateSubscriptionHistoryDto>();
        return autoFaker
            .RuleFor(dto => dto.Steps, f => new List<SubscriptionHistoryStep>())
            .Generate();
    }

    private static UpdateSubscriptionHistoryDto CreateUpdateSubscriptionHistoryDto()
    {
        return new AutoFaker<UpdateSubscriptionHistoryDto>().Generate();
    }

    private static SubscriptionHistoryDto CreateSubscriptionHistoryDto(Guid? id = null, Guid? subscriptionId = null)
    {
        var autoFaker = new AutoFaker<SubscriptionHistoryDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);

        if (subscriptionId.HasValue)
            autoFaker.RuleFor(dto => dto.SubscriptionId, _ => subscriptionId.Value);

        return autoFaker
            .RuleFor(dto => dto.Steps, f => new List<SubscriptionHistoryStep>())
            .Generate();
    }

    private static IReadOnlyCollection<SubscriptionHistoryDto> CreateSubscriptionHistoryDtoList(int count)
    {
        var list = new List<SubscriptionHistoryDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionHistoryDto());
        }
        return list.AsReadOnly();
    }

    #endregion
}