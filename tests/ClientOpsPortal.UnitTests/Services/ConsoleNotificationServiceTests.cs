using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace ClientOpsPortal.UnitTests.Services;

public class ConsoleNotificationServiceTests
{
    private readonly Mock<ILogger<ConsoleNotificationService>> _loggerMock;
    private readonly ConsoleNotificationService _sut;

    public ConsoleNotificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ConsoleNotificationService>>();
        _sut = new ConsoleNotificationService(_loggerMock.Object);
    }

    #region SendPasswordResetAsync Tests

    [Fact]
    public async Task SendPasswordResetAsync_LogsInformationWithEmailAndPassword()
    {
        // Arrange
        var email = "user@example.com";
        var temporaryPassword = "TempPass123!";

        // Act
        await _sut.SendPasswordResetAsync(email, temporaryPassword);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email) &&
                    v.ToString().Contains(temporaryPassword)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_LogsWithCorrectMessageTemplate()
    {
        // Arrange
        var email = "user@example.com";
        var temporaryPassword = "TempPass123!";
        var expectedMessagePart = "Password reset for";

        // Act
        await _sut.SendPasswordResetAsync(email, temporaryPassword);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(expectedMessagePart)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_WithEmptyEmail_LogsEmptyEmail()
    {
        // Arrange
        var email = "";
        var temporaryPassword = "TempPass123!";

        // Act
        await _sut.SendPasswordResetAsync(email, temporaryPassword);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_WithNullEmail_LogsNullEmail()
    {
        // Arrange
        string? email = null;
        var temporaryPassword = "TempPass123!";

        // Act
        await _sut.SendPasswordResetAsync(email!, temporaryPassword);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email ?? "null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_WithEmptyPassword_LogsEmptyPassword()
    {
        // Arrange
        var email = "user@example.com";
        var temporaryPassword = "";

        // Act
        await _sut.SendPasswordResetAsync(email, temporaryPassword);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(temporaryPassword)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_WithNullPassword_LogsNullPassword()
    {
        // Arrange
        var email = "user@example.com";
        string? temporaryPassword = null;

        // Act
        await _sut.SendPasswordResetAsync(email, temporaryPassword!);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(temporaryPassword ?? "null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_ReturnsCompletedTask()
    {
        // Arrange
        var email = "user@example.com";
        var temporaryPassword = "TempPass123!";

        // Act & Assert
        await _sut.SendPasswordResetAsync(email, temporaryPassword);
    }

    #endregion

    #region SendWelcomeWithPasswordAsync Tests

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_LogsInformationWithEmailLoginAndPassword()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var password = "WelcomePass123!";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email) &&
                    v.ToString().Contains(login) &&
                    v.ToString().Contains(password)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_LogsWithCorrectMessageTemplate()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var password = "WelcomePass123!";
        var expectedMessagePart = "created. Login:";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(expectedMessagePart)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_WithEmptyEmail_LogsEmptyEmail()
    {
        // Arrange
        var email = "";
        var login = "testuser";
        var password = "WelcomePass123!";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_WithNullEmail_LogsNullEmail()
    {
        // Arrange
        string? email = null;
        var login = "testuser";
        var password = "WelcomePass123!";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email!, login, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email ?? "null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_WithEmptyLogin_LogsEmptyLogin()
    {
        // Arrange
        var email = "user@example.com";
        var login = "";
        var password = "WelcomePass123!";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(login)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_WithNullLogin_LogsNullLogin()
    {
        // Arrange
        var email = "user@example.com";
        string? login = null;
        var password = "WelcomePass123!";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login!, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(login ?? "null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_WithEmptyPassword_LogsEmptyPassword()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var password = "";

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login, password);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(password)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_WithNullPassword_LogsNullPassword()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        string? password = null;

        // Act
        await _sut.SendWelcomeWithPasswordAsync(email, login, password!);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(password ?? "null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_ReturnsCompletedTask()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var password = "WelcomePass123!";

        // Act & Assert
        await _sut.SendWelcomeWithPasswordAsync(email, login, password);
    }

    #endregion

    #region Multiple Calls Tests

    [Fact]
    public async Task SendPasswordResetAsync_MultipleCalls_LogsEachCall()
    {
        // Arrange
        var calls = new[]
        {
            ("user1@example.com", "Pass1"),
            ("user2@example.com", "Pass2"),
            ("user3@example.com", "Pass3")
        };

        // Act
        foreach (var (email, password) in calls)
        {
            await _sut.SendPasswordResetAsync(email, password);
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(calls.Length));
    }

    [Fact]
    public async Task SendWelcomeWithPasswordAsync_MultipleCalls_LogsEachCall()
    {
        // Arrange
        var calls = new[]
        {
            ("user1@example.com", "login1", "Pass1"),
            ("user2@example.com", "login2", "Pass2"),
            ("user3@example.com", "login3", "Pass3")
        };

        // Act
        foreach (var (email, login, password) in calls)
        {
            await _sut.SendWelcomeWithPasswordAsync(email, login, password);
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(calls.Length));
    }

    #endregion
}