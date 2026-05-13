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

public class SubscriptionsControllerTests
{
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<IContractService> _contractServiceMock;
    private readonly Mock<IAbonentService> _abonentServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly SubscriptionsController _sut;

    public SubscriptionsControllerTests()
    {
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _contractServiceMock = new Mock<IContractService>();
        _abonentServiceMock = new Mock<IAbonentService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new SubscriptionsController(
            _subscriptionServiceMock.Object,
            _contractServiceMock.Object,
            _abonentServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenSubscriptionsExist_ReturnsOkWithSubscriptions()
    {
        // Arrange
        var expectedSubscriptions = CreateSubscriptionDtoList(5);

        _subscriptionServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscriptions);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscriptions = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionDto>>();
        subscriptions.Count().ShouldBe(5);

        _subscriptionServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoSubscriptionsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<SubscriptionDto>().AsReadOnly();

        _subscriptionServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscriptions = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionDto>>();
        subscriptions.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedSubscriptions = CreateSubscriptionDtoList(3);

        _subscriptionServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscriptions);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _subscriptionServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenSubscriptionExists_ReturnsOkWithSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var expectedSubscription = CreateSubscriptionDto(subscriptionId);

        _subscriptionServiceMock
            .Setup(s => s.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscription);

        // Act
        var result = await _sut.GetById(subscriptionId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscription = okResult.Value.ShouldBeOfType<SubscriptionDto>();

        subscription.Id.ShouldBe(subscriptionId);

        _subscriptionServiceMock.Verify(
            s => s.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenSubscriptionNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionServiceMock
            .Setup(s => s.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionDto?)null);

        // Act
        var result = await _sut.GetById(subscriptionId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(subscriptionId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");

        _subscriptionServiceMock.Verify(
            s => s.GetByIdAsync(subscriptionId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToService()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var expectedSubscription = CreateSubscriptionDto(subscriptionId);

        _subscriptionServiceMock
            .Setup(s => s.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscription);

        // Act
        var result = await _sut.GetById(subscriptionId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _subscriptionServiceMock.Verify(
            s => s.GetByIdAsync(subscriptionId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetFullSubscriptionData Tests

    [Fact]
    public async Task GetFullSubscriptionData_WhenSubscriptionExists_ReturnsOkWithFullData()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var expectedFullData = CreateSubscriptionFullDataDto();

        _subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFullData);

        // Act
        var result = await _sut.GetFullSubscriptionData(subscriptionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var fullData = okResult.Value.ShouldBeOfType<SubscriptionFullDataDto>();

        fullData.ShouldNotBeNull();

        _subscriptionServiceMock.Verify(
            s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFullSubscriptionData_WhenSubscriptionNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        _subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionFullDataDto?)null);

        // Act
        var result = await _sut.GetFullSubscriptionData(subscriptionId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(subscriptionId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region GetByContract Tests

    [Fact]
    public async Task GetByContract_WhenSubscriptionsExist_ReturnsOkWithSubscriptions()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var expectedSubscriptions = CreateSubscriptionFullDataDtoList(3); // Используем правильный тип

        _subscriptionServiceMock
            .Setup(s => s.GetActiveSubscriptionsByContractAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscriptions);

        // Act
        var result = await _sut.GetByContract(contractId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscriptions = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionFullDataDto>>();
        subscriptions.Count().ShouldBe(3);

        _subscriptionServiceMock.Verify(
            s => s.GetActiveSubscriptionsByContractAsync(contractId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByContract_WhenNoSubscriptions_ReturnsOkWithEmptyList()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var emptyCollection = new List<SubscriptionFullDataDto>().AsReadOnly();

        _subscriptionServiceMock
            .Setup(s => s.GetActiveSubscriptionsByContractAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyCollection);

        // Act
        var result = await _sut.GetByContract(contractId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscriptions = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionFullDataDto>>();
        subscriptions.ShouldBeEmpty();
    }

    #endregion

    #region GetByAbonentId Tests

    [Fact]
    public async Task GetByAbonentId_WhenManager_ReturnsOkWithSubscriptions()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var expectedSubscriptions = CreateSubscriptionFullDataDtoList(4);

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.GetSubscriptionsByAbonentIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscriptions);

        // Act
        var result = await _sut.GetByAbonentId(abonentId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscriptions = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionFullDataDto>>();
        subscriptions.Count().ShouldBe(4);

        _subscriptionServiceMock.Verify(
            s => s.GetSubscriptionsByAbonentIdAsync(abonentId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByAbonentId_WhenAbonentRoleAndOwnsAbonent_ReturnsOkWithSubscriptions()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedSubscriptions = CreateSubscriptionFullDataDtoList(2); // Используем правильный тип
        var existingAbonent = CreateAbonentDto(abonentId, userId: userId);

        SetupUserRole("Abonent", userId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        _subscriptionServiceMock
            .Setup(s => s.GetSubscriptionsByAbonentIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscriptions); // Теперь тип правильный

        // Act
        var result = await _sut.GetByAbonentId(abonentId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscriptions = okResult.Value.ShouldBeAssignableTo<IEnumerable<SubscriptionFullDataDto>>(); // Исправлен тип
        subscriptions.Count().ShouldBe(2);

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);

        _subscriptionServiceMock.Verify(
            s => s.GetSubscriptionsByAbonentIdAsync(abonentId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByAbonentId_WhenAbonentRoleAndDoesNotOwnAbonent_ReturnsForbid()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var existingAbonent = CreateAbonentDto(abonentId, userId: differentUserId);

        SetupUserRole("Abonent", currentUserId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        // Act
        var result = await _sut.GetByAbonentId(abonentId, true);

        // Assert
        result.ShouldBeOfType<ForbidResult>();

        _subscriptionServiceMock.Verify(
            s => s.GetSubscriptionsByAbonentIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByAbonentId_WhenNoSubscriptions_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var emptyCollection = new List<SubscriptionFullDataDto>().AsReadOnly();

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.GetSubscriptionsByAbonentIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyCollection);

        // Act
        var result = await _sut.GetByAbonentId(abonentId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(abonentId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдены");
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenManager_ReturnsCreatedAtActionWithSubscription()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();
        var createdDto = CreateSubscriptionDto(id: Guid.NewGuid());

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(SubscriptionsController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<SubscriptionDto>().ShouldBe(createdDto);

        _subscriptionServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenAbonentRoleAndOwnsContract_ReturnsCreatedAtActionWithSubscription()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();
        var createdDto = CreateSubscriptionDto(id: Guid.NewGuid());
        var userId = Guid.NewGuid();
        var contract = CreateContractDto(createDto.ContractId);
        var abonent = CreateAbonentDto(contract.AbonentId, userId: userId);

        SetupUserRole("Abonent", userId);

        _contractServiceMock
            .Setup(s => s.GetByIdAsync(createDto.ContractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(contract.AbonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        _subscriptionServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(SubscriptionsController.GetById));
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);

        _subscriptionServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenAbonentRoleAndDoesNotOwnContract_ReturnsForbid()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();
        var currentUserId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var contract = CreateContractDto(createDto.ContractId);
        var abonent = CreateAbonentDto(contract.AbonentId, userId: differentUserId);

        SetupUserRole("Abonent", currentUserId);

        _contractServiceMock
            .Setup(s => s.GetByIdAsync(createDto.ContractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(contract.AbonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.ShouldBeOfType<ForbidResult>();

        _subscriptionServiceMock.Verify(
            s => s.CreateAsync(It.IsAny<SubscriptionDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_WhenContractNotFound_ReturnsBadRequest()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();

        SetupUserRole("Abonent", Guid.NewGuid());

        _contractServiceMock
            .Setup(s => s.GetByIdAsync(createDto.ContractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDataDto?)null);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain("Договор не найден");

        _subscriptionServiceMock.Verify(
            s => s.CreateAsync(It.IsAny<SubscriptionDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateSubscriptionDto();
        var errorMessage = "Ошибка создания подписки";

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenManager_ReturnsOkWithUpdatedSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionDto();
        var updatedDto = CreateSubscriptionDto(subscriptionId);

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.UpdateAsync(subscriptionId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(subscriptionId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscription = okResult.Value.ShouldBeOfType<SubscriptionDto>();

        subscription.Id.ShouldBe(subscriptionId);

        _subscriptionServiceMock.Verify(
            s => s.UpdateAsync(subscriptionId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenSubscriptionNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var updateDto = CreateUpdateSubscriptionDto();

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.UpdateAsync(subscriptionId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Subscription), subscriptionId));

        // Act
        var result = await _sut.Update(subscriptionId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(subscriptionId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region ChangeTariffPlan Tests

    [Fact]
    public async Task ChangeTariffPlan_WhenManager_ReturnsOkWithUpdatedSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var newTariffPlanId = Guid.NewGuid();
        var updatedDto = CreateSubscriptionDto(subscriptionId);

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.ChangeTariffPlanAsync(subscriptionId, newTariffPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.ChangeTariffPlan(subscriptionId, newTariffPlanId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscription = okResult.Value.ShouldBeOfType<SubscriptionDto>();

        subscription.Id.ShouldBe(subscriptionId);

        _subscriptionServiceMock.Verify(
            s => s.ChangeTariffPlanAsync(subscriptionId, newTariffPlanId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeTariffPlan_WhenSubscriptionNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var newTariffPlanId = Guid.NewGuid();

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.ChangeTariffPlanAsync(subscriptionId, newTariffPlanId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Subscription), subscriptionId));

        // Act
        var result = await _sut.ChangeTariffPlan(subscriptionId, newTariffPlanId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(subscriptionId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region CancelSubscription Tests

    [Fact]
    public async Task CancelSubscription_WhenManager_ReturnsOkWithCancelledSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var cancelledDto = CreateSubscriptionDto(subscriptionId);

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.CancelSubscriptionAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledDto);

        // Act
        var result = await _sut.CancelSubscription(subscriptionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var subscription = okResult.Value.ShouldBeOfType<SubscriptionDto>();

        subscription.Id.ShouldBe(subscriptionId);

        _subscriptionServiceMock.Verify(
            s => s.CancelSubscriptionAsync(subscriptionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelSubscription_WhenSubscriptionNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.CancelSubscriptionAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Subscription), subscriptionId));

        // Act
        var result = await _sut.CancelSubscription(subscriptionId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(subscriptionId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenSubscriptionExists_ReturnsNoContent()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.DeleteAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(subscriptionId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _subscriptionServiceMock.Verify(
            s => s.DeleteAsync(subscriptionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenSubscriptionNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        SetupUserRole("Manager");

        _subscriptionServiceMock
            .Setup(s => s.DeleteAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Subscription), subscriptionId));

        // Act
        var result = await _sut.Delete(subscriptionId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(subscriptionId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найдена");
    }

    #endregion

    #region Helper Methods

    private void SetupUserRole(string role, Guid? userId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role)
        };

        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(userId);
    }

    private static SubscriptionDto CreateSubscriptionDto(Guid? id = null, Guid? contractId = null)
    {
        var autoFaker = new AutoFaker<SubscriptionDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);

        if (contractId.HasValue)
            autoFaker.RuleFor(dto => dto.ContractId, _ => contractId.Value);

        return autoFaker.Generate();
    }

    private static IReadOnlyCollection<SubscriptionDto> CreateSubscriptionDtoList(int count)
    {
        var list = new List<SubscriptionDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionDto());
        }
        return list.AsReadOnly();
    }

    private static SubscriptionFullDataDto CreateSubscriptionFullDataDto()
    {
        return new AutoFaker<SubscriptionFullDataDto>().Generate();
    }

    private static UpdateSubscriptionDto CreateUpdateSubscriptionDto()
    {
        return new AutoFaker<UpdateSubscriptionDto>().Generate();
    }

    private static ContractDataDto CreateContractDto(Guid? id = null, Guid? abonentId = null)
    {
        var autoFaker = new AutoFaker<ContractDataDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);

        if (abonentId.HasValue)
            autoFaker.RuleFor(dto => dto.AbonentId, _ => abonentId.Value);

        return autoFaker.Generate();
    }

    private static AbonentDto CreateAbonentDto(Guid? id = null, Guid? userId = null)
    {
        var autoFaker = new AutoFaker<AbonentDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);

        if (userId.HasValue)
            autoFaker.RuleFor(dto => dto.UserId, _ => userId.Value);

        return autoFaker.Generate();
    }

    private static IReadOnlyCollection<SubscriptionFullDataDto> CreateSubscriptionFullDataDtoList(int count)
    {
        var list = new List<SubscriptionFullDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateSubscriptionFullDataDto());
        }
        return list.AsReadOnly();
    }

    #endregion
}