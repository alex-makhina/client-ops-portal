using AutoBogus;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly IdentityService _sut;

    public IdentityServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();
        _sut = new IdentityService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _userRepositoryMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        return userManagerMock;
    }

    private static Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null);
        return roleManagerMock;
    }

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WhenValidData_CreatesUserAndReturnsUser()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "Abonent";
        var appUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, _) => user.Id = appUserId);

        _roleManagerMock
            .Setup(m => m.RoleExistsAsync(role))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateUserAsync(userName, email, password, role);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);
        result.ExternalId.ShouldBe(appUserId.ToString());
        result.IdentityProvider.ShouldBe("Identity");

        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
        _roleManagerMock.Verify(m => m.RoleExistsAsync(role), Times.Once);
        _roleManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationRole>()), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), role), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleAlreadyExists_DoesNotCreateRole()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "Admin";

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(m => m.RoleExistsAsync(role))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateUserAsync(userName, email, password, role);

        // Assert
        result.ShouldNotBeNull();

        _roleManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "Abonent";
        var errors = new[] { new IdentityError { Description = "Error 1" }, new IdentityError { Description = "Error 2" } };

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateUserAsync(userName, email, password, role));

        exception.Message.ShouldContain("Failed to create user");
        exception.Message.ShouldContain("Error 1");
        exception.Message.ShouldContain("Error 2");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesApplicationUserWithCorrectProperties()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "Abonent";
        ApplicationUser? createdUser = null;

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .Callback<ApplicationUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(m => m.RoleExistsAsync(role))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateUserAsync(userName, email, password, role);

        // Assert
        createdUser.ShouldNotBeNull();
        createdUser.UserName.ShouldBe(userName);
        createdUser.Email.ShouldBe(email);
        createdUser.EmailConfirmed.ShouldBeTrue();
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WhenUserExists_ResetsPasswordAndReturnsNewPassword()
    {
        // Arrange
        var userName = "testuser";
        var appUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = userName };
        var resetToken = "reset-token-123";
        var newPassword = "GeneratedPass123!";

        _userManagerMock
            .Setup(m => m.FindByNameAsync(userName))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(appUser))
            .ReturnsAsync(resetToken);

        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(appUser, resetToken, It.IsAny<string>()))
            .Callback<ApplicationUser, string, string>((_, _, newPwd) => newPassword = newPwd)
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ResetPasswordAsync(userName);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.Length.ShouldBeGreaterThanOrEqualTo(12);
        result.Length.ShouldBeLessThanOrEqualTo(16);

        _userManagerMock.Verify(m => m.FindByNameAsync(userName), Times.Once);
        _userManagerMock.Verify(m => m.GeneratePasswordResetTokenAsync(appUser), Times.Once);
        _userManagerMock.Verify(m => m.ResetPasswordAsync(appUser, resetToken, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "nonexistent";

        _userManagerMock
            .Setup(m => m.FindByNameAsync(userName))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.ResetPasswordAsync(userName));

        exception.Message.ShouldContain($"User '{userName}' not found");

        _userManagerMock.Verify(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenResetFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "testuser";
        var appUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = userName };
        var resetToken = "reset-token-123";
        var errors = new[] { new IdentityError { Description = "Reset failed" } };

        _userManagerMock
            .Setup(m => m.FindByNameAsync(userName))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(appUser))
            .ReturnsAsync(resetToken);

        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(appUser, resetToken, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.ResetPasswordAsync(userName));

        exception.Message.ShouldContain("Failed to reset password");
        exception.Message.ShouldContain("Reset failed");
    }

    #endregion

    #region GenerateRandomPassword Tests

    [Fact]
    public void GenerateRandomPassword_ReturnsPasswordWithValidLength()
    {
        // Act
        var result = _sut.GenerateRandomPassword();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.Length.ShouldBeGreaterThanOrEqualTo(12);
        result.Length.ShouldBeLessThanOrEqualTo(16);
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneLowercaseLetter()
    {
        // Act
        var result = _sut.GenerateRandomPassword();

        // Assert
        result.ShouldContain(c => char.IsLower(c));
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneUppercaseLetter()
    {
        // Act
        var result = _sut.GenerateRandomPassword();

        // Assert
        result.ShouldContain(c => char.IsUpper(c));
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneDigit()
    {
        // Act
        var result = _sut.GenerateRandomPassword();

        // Assert
        result.ShouldContain(c => char.IsDigit(c));
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneSpecialCharacter()
    {
        // Act
        var result = _sut.GenerateRandomPassword();
        var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        // Assert
        result.ShouldContain(c => specialChars.Contains(c));
    }

    [Fact]
    public void GenerateRandomPassword_ReturnsDifferentPasswordsOnMultipleCalls()
    {
        // Act
        var password1 = _sut.GenerateRandomPassword();
        var password2 = _sut.GenerateRandomPassword();
        var password3 = _sut.GenerateRandomPassword();

        // Assert
        password1.ShouldNotBe(password2);
        password1.ShouldNotBe(password3);
        password2.ShouldNotBe(password3);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task CreateUserAsync_ThenResetPasswordAsync_CompleteUserLifecycle()
    {
        // Arrange
        var userName = "lifecycleuser";
        var email = "lifecycle@example.com";
        var password = "InitialPass123!";
        var role = "Abonent";
        var appUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .Callback<ApplicationUser, string>((user, _) => user.Id = appUserId)
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(m => m.RoleExistsAsync(role))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManagerMock
            .Setup(m => m.FindByNameAsync(userName))
            .ReturnsAsync(new ApplicationUser { Id = appUserId, UserName = userName });

        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("reset-token");

        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var createdUser = await _sut.CreateUserAsync(userName, email, password, role);
        var newPassword = await _sut.ResetPasswordAsync(userName);

        // Assert
        createdUser.ShouldNotBeNull();
        newPassword.ShouldNotBeNullOrEmpty();
        newPassword.ShouldNotBe(password);
    }

    #endregion
}