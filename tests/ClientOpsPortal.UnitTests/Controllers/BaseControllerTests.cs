using ClientOpsPortal.Api.Controllers;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Security.Claims;

namespace ClientOpsPortal.UnitTests.Controllers;

public class TestBaseController : BaseController
{
    public TestBaseController(ICurrentUserService currentUserService)
        : base(currentUserService)
    {
    }

    public new Task<bool> IsAbonentOwnerAsync(Guid abonentId, IAbonentService abonentService, CancellationToken ct = default)
    => base.IsAbonentOwnerAsync(abonentId, abonentService, ct);

    public new Task<bool> IsContractOwnerAsync(Guid contractId, IContractService contractService, IAbonentService abonentService, CancellationToken ct = default)
        => base.IsContractOwnerAsync(contractId, contractService, abonentService, ct);

    public new Task<bool> IsSubscriptionOwnerAsync(Guid subscriptionId, ISubscriptionService subscriptionService, IContractService contractService, IAbonentService abonentService, CancellationToken ct = default)
        => base.IsSubscriptionOwnerAsync(subscriptionId, subscriptionService, contractService, abonentService, ct);

    public new bool IsCurrentUserAbonentOwner(Guid abonentUserId)
        => base.IsCurrentUserAbonentOwner(abonentUserId);
}

public class BaseControllerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TestBaseController _sut;

    public BaseControllerTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new TestBaseController(_currentUserServiceMock.Object);
    }

    private void SetupUserRole(string role, Guid? userId = null)
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

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

    private static AbonentDto CreateAbonentDto(Guid id, Guid userId)
    {
        return new AbonentDto
        {
            Id = id,
            UserId = userId,
            IdentificationNumber = "1234567890",
            FirstName = "Иван",
            LastName = "Иванов",
            MiddleName = "Иванович",
            AccountNumber = "ACC-1234567890"
        };
    }

    #region IsCurrentUserAbonentOwner Tests

    [Fact]
    public void IsCurrentUserAbonentOwner_WhenUserIsManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Manager");
        var abonentUserId = Guid.NewGuid();

        // Act
        var result = _sut.IsCurrentUserAbonentOwner(abonentUserId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsCurrentUserAbonentOwner_WhenUserIsAdmin_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Admin");
        var abonentUserId = Guid.NewGuid();

        // Act
        var result = _sut.IsCurrentUserAbonentOwner(abonentUserId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsCurrentUserAbonentOwner_WhenUserIsServiceManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("ServiceManager");
        var abonentUserId = Guid.NewGuid();

        // Act
        var result = _sut.IsCurrentUserAbonentOwner(abonentUserId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsCurrentUserAbonentOwner_WhenUserIsAbonentAndOwnsAbonent_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserRole("Abonent", userId);

        // Act
        var result = _sut.IsCurrentUserAbonentOwner(userId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsCurrentUserAbonentOwner_WhenUserIsAbonentAndDoesNotOwnAbonent_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        SetupUserRole("Abonent", currentUserId);

        // Act
        var result = _sut.IsCurrentUserAbonentOwner(otherUserId);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region IsAbonentOwnerAsync Tests

    [Fact]
    public async Task IsAbonentOwnerAsync_WhenUserIsManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Manager");
        var abonentId = Guid.NewGuid();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsAbonentOwnerAsync(abonentId, abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsAbonentOwnerAsync_WhenUserIsAdmin_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Admin");
        var abonentId = Guid.NewGuid();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsAbonentOwnerAsync(abonentId, abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsAbonentOwnerAsync_WhenUserIsServiceManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("ServiceManager");
        var abonentId = Guid.NewGuid();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsAbonentOwnerAsync(abonentId, abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsAbonentOwnerAsync_WhenUserIsAbonentAndOwnsAbonent_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();
        var abonent = CreateAbonentDto(abonentId, userId);

        SetupUserRole("Abonent", userId);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.IsAbonentOwnerAsync(abonentId, abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsAbonentOwnerAsync_WhenUserIsAbonentAndDoesNotOwnAbonent_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();
        var abonent = CreateAbonentDto(abonentId, otherUserId);

        SetupUserRole("Abonent", currentUserId);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.IsAbonentOwnerAsync(abonentId, abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsAbonentOwnerAsync_WhenAbonentNotFound_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();

        SetupUserRole("Abonent", currentUserId);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AbonentDto?)null);

        // Act
        var result = await _sut.IsAbonentOwnerAsync(abonentId, abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region IsContractOwnerAsync Tests

    private static ContractDataDto CreateContractDto(Guid id, Guid abonentId)
    {
        return new ContractDataDto
        {
            Id = id,
            ContractNumber = $"CT-{DateTime.Now:yyyyMMdd}-001",
            AbonentId = abonentId,
            BeginDate = DateTimeOffset.UtcNow,
            EndDate = null
        };
    }

    [Fact]
    public async Task IsContractOwnerAsync_WhenUserIsManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Manager");
        var contractId = Guid.NewGuid();
        var contractServiceMock = new Mock<IContractService>();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsContractOwnerAsync(contractId, contractServiceMock.Object, abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        contractServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsContractOwnerAsync_WhenUserIsAbonentAndOwnsContract_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();
        var contract = CreateContractDto(contractId, abonentId);
        var abonent = CreateAbonentDto(abonentId, userId);

        SetupUserRole("Abonent", userId);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.IsContractOwnerAsync(contractId, contractServiceMock.Object, abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        contractServiceMock.Verify(
            s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()),
            Times.Once);

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsContractOwnerAsync_WhenUserIsAbonentAndDoesNotOwnContract_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();
        var contract = CreateContractDto(contractId, abonentId);
        var abonent = CreateAbonentDto(abonentId, otherUserId);

        SetupUserRole("Abonent", currentUserId);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.IsContractOwnerAsync(contractId, contractServiceMock.Object, abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsContractOwnerAsync_WhenContractNotFound_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        SetupUserRole("Abonent", currentUserId);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDataDto?)null);

        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsContractOwnerAsync(contractId, contractServiceMock.Object, abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsContractOwnerAsync_WhenAbonentNotFound_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();
        var contract = CreateContractDto(contractId, abonentId);

        SetupUserRole("Abonent", currentUserId);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AbonentDto?)null);

        // Act
        var result = await _sut.IsContractOwnerAsync(contractId, contractServiceMock.Object, abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region IsSubscriptionOwnerAsync Tests

    private static SubscriptionFullDataDto CreateSubscriptionFullDataDto(Guid contractId)
    {
        return new SubscriptionFullDataDto
        {
            ContractId = contractId,
            ServiceId = Guid.NewGuid(),
            ServiceName = "Test Service",
            TariffPlanId = Guid.NewGuid(),
            TariffPlanName = "Test Tariff",
            BeginDate = DateTimeOffset.UtcNow,
            EndDate = null
        };
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenUserIsManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Manager");
        var subscriptionId = Guid.NewGuid();
        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        var contractServiceMock = new Mock<IContractService>();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        subscriptionServiceMock.Verify(
            s => s.GetFullSubscriptionDataAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenUserIsAdmin_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("Admin");
        var subscriptionId = Guid.NewGuid();
        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        var contractServiceMock = new Mock<IContractService>();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenUserIsServiceManager_ReturnsTrue()
    {
        // Arrange
        SetupUserRole("ServiceManager");
        var subscriptionId = Guid.NewGuid();
        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        var contractServiceMock = new Mock<IContractService>();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenUserIsAbonentAndOwnsSubscription_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();

        var subscription = CreateSubscriptionFullDataDto(contractId);
        var contract = CreateContractDto(contractId, abonentId);
        var abonent = CreateAbonentDto(abonentId, userId);

        SetupUserRole("Abonent", userId);

        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeTrue();

        subscriptionServiceMock.Verify(
            s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()),
            Times.Once);

        contractServiceMock.Verify(
            s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()),
            Times.Once);

        abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenUserIsAbonentAndDoesNotOwnSubscription_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();

        var subscription = CreateSubscriptionFullDataDto(contractId);
        var contract = CreateContractDto(contractId, abonentId);
        var abonent = CreateAbonentDto(abonentId, otherUserId);

        SetupUserRole("Abonent", currentUserId);

        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenSubscriptionNotFound_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        SetupUserRole("Abonent", currentUserId);

        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionFullDataDto?)null);

        var contractServiceMock = new Mock<IContractService>();
        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenContractNotFound_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        var subscription = CreateSubscriptionFullDataDto(contractId);

        SetupUserRole("Abonent", currentUserId);

        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDataDto?)null);

        var abonentServiceMock = new Mock<IAbonentService>();

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsSubscriptionOwnerAsync_WhenAbonentNotFound_ReturnsFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var abonentId = Guid.NewGuid();

        var subscription = CreateSubscriptionFullDataDto(contractId);
        var contract = CreateContractDto(contractId, abonentId);

        SetupUserRole("Abonent", currentUserId);

        var subscriptionServiceMock = new Mock<ISubscriptionService>();
        subscriptionServiceMock
            .Setup(s => s.GetFullSubscriptionDataAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var contractServiceMock = new Mock<IContractService>();
        contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var abonentServiceMock = new Mock<IAbonentService>();
        abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AbonentDto?)null);

        // Act
        var result = await _sut.IsSubscriptionOwnerAsync(
            subscriptionId,
            subscriptionServiceMock.Object,
            contractServiceMock.Object,
            abonentServiceMock.Object);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion
}