using AutoBogus;
using ClientOpsPortal.Services.Auth.Contracts;
using ClientOpsPortal.Services.Auth.Controllers;
using ClientOpsPortal.Services.Auth.Domain;
using ClientOpsPortal.Services.Auth.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Timers;

namespace ClientOpsPortal.Services.Auth.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();

        var jwtSettings = new JwtSettings
        {
            SecretKey = "this-is-a-very-long-secret-key-for-testing-purposes-1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };
        _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

        _sut = new AuthController(_userManagerMock.Object, _jwtSettingsMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    #region Login Tests

    [Fact]
    public async Task Login_WhenValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var password = "password123";
        var roles = new List<string> { "Manager" };

        var user = CreateApplicationUser(userId, userName);

        var loginRequest = new LoginRequest
        {
            Login = userName,
            Password = password
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginRequest.Login))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.Login(loginRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<AuthResponse>();

        response.ShouldNotBeNull();
        response.Token.ShouldNotBeNullOrEmpty();
        response.UserId.ShouldBe(userId.ToString());
        response.UserName.ShouldBe(userName);
        response.Roles.ShouldBe(roles);

        _userManagerMock.Verify(x => x.FindByNameAsync(loginRequest.Login), Times.Once);
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, loginRequest.Password), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Exactly(2));
    }

    [Fact]
    public async Task Login_WhenUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "nonexistent",
            Password = "password123"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginRequest.Login))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.ShouldBeOfType<UnauthorizedObjectResult>();
        unauthorizedResult.Value.ShouldNotBeNull();
        unauthorizedResult.Value.ToString().ShouldContain("Invalid username or password");

        _userManagerMock.Verify(x => x.FindByNameAsync(loginRequest.Login), Times.Once);
        _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenPasswordIsInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);

        var loginRequest = new LoginRequest
        {
            Login = userName,
            Password = "wrongpassword"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginRequest.Login))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.ShouldBeOfType<UnauthorizedObjectResult>();
        unauthorizedResult.Value.ShouldNotBeNull();
        unauthorizedResult.Value.ToString().ShouldContain("Invalid username or password");

        _userManagerMock.Verify(x => x.FindByNameAsync(loginRequest.Login), Times.Once);
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, loginRequest.Password), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenUserIsLockedOut_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);

        var loginRequest = new LoginRequest
        {
            Login = userName,
            Password = "password123"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginRequest.Login))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.ShouldBeOfType<UnauthorizedObjectResult>();
        unauthorizedResult.Value.ShouldNotBeNull();
        unauthorizedResult.Value.ToString().ShouldContain("User is blocked");

        _userManagerMock.Verify(x => x.IsLockedOutAsync(user), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenUserHasMultipleRoles_TokenContainsAllRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var password = "password123";
        var roles = new List<string> { "Manager", "ServiceManager", "Abonent" };

        var user = CreateApplicationUser(userId, userName);

        var loginRequest = new LoginRequest
        {
            Login = userName,
            Password = password
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginRequest.Login))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.Login(loginRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<AuthResponse>();

        response.Roles.Count.ShouldBe(3);
        response.Roles.ShouldContain("Manager");
        response.Roles.ShouldContain("ServiceManager");
        response.Roles.ShouldContain("Abonent");
    }

    [Fact]
    public async Task Login_WhenUserHasNoRoles_ReturnsEmptyRolesList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var password = "password123";
        var roles = new List<string>();

        var user = CreateApplicationUser(userId, userName);

        var loginRequest = new LoginRequest
        {
            Login = userName,
            Password = password
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(loginRequest.Login))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.Login(loginRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<AuthResponse>();

        response.Roles.ShouldBeEmpty();
    }

    #endregion

    #region ForgotPassword Tests

    [Fact]
    public async Task ForgotPassword_WhenUserExists_ReturnsOkWithTemporaryPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);
        var resetToken = "reset-token-123";

        var forgotRequest = new ForgotPasswordRequest
        {
            LoginIdentifier = userName
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(forgotRequest.LoginIdentifier))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);

        // Act
        var result = await _sut.ForgotPassword(forgotRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<ForgotPasswordResponse>();

        response.TemporaryPassword.ShouldBe(resetToken);

        _userManagerMock.Verify(x => x.FindByNameAsync(forgotRequest.LoginIdentifier), Times.Once);
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var forgotRequest = new ForgotPasswordRequest
        {
            LoginIdentifier = "nonexistent"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(forgotRequest.LoginIdentifier))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ForgotPassword(forgotRequest);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("User not found");

        _userManagerMock.Verify(x => x.FindByNameAsync(forgotRequest.LoginIdentifier), Times.Once);
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    #endregion

    #region SetPassword Tests

    [Fact]
    public async Task SetPassword_WhenValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);
        var token = "reset-token-123";
        var newPassword = "NewPass456!";

        var setPasswordRequest = new SetPasswordRequest
        {
            LoginIdentifier = userName,
            Token = token,
            NewPassword = newPassword
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(setPasswordRequest.LoginIdentifier))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.SetPassword(setPasswordRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldNotBeNull();

        _userManagerMock.Verify(x => x.FindByNameAsync(setPasswordRequest.LoginIdentifier), Times.Once);
        _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }

    [Fact]
    public async Task SetPassword_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var setPasswordRequest = new SetPasswordRequest
        {
            LoginIdentifier = "nonexistent",
            Token = "token",
            NewPassword = "NewPass456!"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(setPasswordRequest.LoginIdentifier))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.SetPassword(setPasswordRequest);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("User not found");

        _userManagerMock.Verify(x => x.FindByNameAsync(setPasswordRequest.LoginIdentifier), Times.Once);
        _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetPassword_WhenResetFails_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);
        var token = "reset-token-123";
        var newPassword = "NewPass456!";
        var errors = new[] { new IdentityError { Description = "Invalid token" } };

        var setPasswordRequest = new SetPasswordRequest
        {
            LoginIdentifier = userName,
            Token = token,
            NewPassword = newPassword
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(setPasswordRequest.LoginIdentifier))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _sut.SetPassword(setPasswordRequest);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();

        _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }

    #endregion

    #region ResetPassword Tests

    [Fact]
    public async Task ResetPassword_WhenValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);
        var currentPassword = "OldPass123!";
        var newPassword = "NewPass456!";

        var resetRequest = new ResetPasswordRequest
        {
            LoginIdentifier = userName,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(resetRequest.LoginIdentifier))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, currentPassword, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ResetPassword(resetRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldNotBeNull();

        _userManagerMock.Verify(x => x.FindByNameAsync(resetRequest.LoginIdentifier), Times.Once);
        _userManagerMock.Verify(x => x.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var resetRequest = new ResetPasswordRequest
        {
            LoginIdentifier = "nonexistent",
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(resetRequest.LoginIdentifier))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ResetPassword(resetRequest);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain("User not found");

        _userManagerMock.Verify(x => x.FindByNameAsync(resetRequest.LoginIdentifier), Times.Once);
        _userManagerMock.Verify(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WhenChangePasswordFails_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var user = CreateApplicationUser(userId, userName);
        var currentPassword = "WrongOldPass123!";
        var newPassword = "NewPass456!";
        var errors = new List<IdentityError>
        {
            new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password" },
            new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak" }
        };

        var resetRequest = new ResetPasswordRequest
        {
            LoginIdentifier = userName,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(resetRequest.LoginIdentifier))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, currentPassword, newPassword))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        // Act
        var result = await _sut.ResetPassword(resetRequest);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();

        _userManagerMock.Verify(x => x.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static ApplicationUser CreateApplicationUser(Guid id, string userName, string? email = null)
    {
        return new AutoFaker<ApplicationUser>()
            .RuleFor(u => u.Id, _ => id)
            .RuleFor(u => u.UserName, _ => userName)
            .RuleFor(u => u.Email, _ => email ?? $"{userName}@example.com")
            .RuleFor(u => u.EmailConfirmed, _ => true)
            .Generate();
    }

    #endregion
}