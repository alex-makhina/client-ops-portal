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

    #region SendPasswordSetLinkAsync Tests

    [Fact]
    public async Task SendPasswordSetLinkAsync_LogsInformationWithEmailLoginAndResetLink()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var resetLink = "http://example.com/reset?token=abc123";

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(email) &&
                    v.ToString().Contains(login) &&
                    v.ToString().Contains(resetLink)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordSetLinkAsync_LogsWithCorrectMessageTemplate()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var resetLink = "http://example.com/reset?token=abc123";
        var expectedMessagePart = "Password set link for";

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink);

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
    public async Task SendPasswordSetLinkAsync_WithEmptyEmail_LogsEmptyEmail()
    {
        // Arrange
        var email = "";
        var login = "testuser";
        var resetLink = "http://example.com/reset?token=abc123";

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink);

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
    public async Task SendPasswordSetLinkAsync_WithNullEmail_LogsNullEmail()
    {
        // Arrange
        string? email = null;
        var login = "testuser";
        var resetLink = "http://example.com/reset?token=abc123";

        // Act
        await _sut.SendPasswordSetLinkAsync(email!, login, resetLink);

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
    public async Task SendPasswordSetLinkAsync_WithEmptyLogin_LogsEmptyLogin()
    {
        // Arrange
        var email = "user@example.com";
        var login = "";
        var resetLink = "http://example.com/reset?token=abc123";

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink);

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
    public async Task SendPasswordSetLinkAsync_WithNullLogin_LogsNullLogin()
    {
        // Arrange
        var email = "user@example.com";
        string? login = null;
        var resetLink = "http://example.com/reset?token=abc123";

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login!, resetLink);

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
    public async Task SendPasswordSetLinkAsync_WithEmptyResetLink_LogsEmptyResetLink()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var resetLink = "";

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(resetLink)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordSetLinkAsync_WithNullResetLink_LogsNullResetLink()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        string? resetLink = null;

        // Act
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink!);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains(resetLink ?? "null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordSetLinkAsync_ReturnsCompletedTask()
    {
        // Arrange
        var email = "user@example.com";
        var login = "testuser";
        var resetLink = "http://example.com/reset?token=abc123";

        // Act & Assert
        await _sut.SendPasswordSetLinkAsync(email, login, resetLink);
    }

    #endregion

    #region Multiple Calls Tests

    [Fact]
    public async Task SendPasswordSetLinkAsync_MultipleCalls_LogsEachCall()
    {
        // Arrange
        var calls = new[]
        {
            ("user1@example.com", "login1", "http://example.com/reset?token=abc"),
            ("user2@example.com", "login2", "http://example.com/reset?token=def"),
            ("user3@example.com", "login3", "http://example.com/reset?token=ghi")
        };

        // Act
        foreach (var (email, login, resetLink) in calls)
        {
            await _sut.SendPasswordSetLinkAsync(email, login, resetLink);
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
