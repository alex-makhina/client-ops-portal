using AutoBogus;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class IdentityServiceTests : IDisposable
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

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });
    }

    public void Dispose()
    {
        _userManagerMock.Reset();
        _roleManagerMock.Reset();
        _userRepositoryMock.Reset();
    }

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WhenValidData_ReturnsUser()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "Manager";
        var applicationUserId = Guid.NewGuid();

        var appUser = new ApplicationUser
        {
            Id = applicationUserId,
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = applicationUserId;
            });

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(role))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateUserAsync(userName, email, password, role);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);
        result.IdentityProvider.ShouldBe("Identity");
        result.ExternalId.ShouldBe(applicationUserId.ToString());

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), password),
            Times.Once);

        _roleManagerMock.Verify(
            x => x.RoleExistsAsync(role),
            Times.Once);

        _userManagerMock.Verify(
            x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleDoesNotExist_CreatesRole()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "NewRole";

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(role))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateUserAsync(userName, email, password, role);

        // Assert
        result.ShouldNotBeNull();

        _roleManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationRole>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";
        var password = "Test123!";
        var role = "Manager";
        var errorDescription = "Password too weak";

        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = errorDescription }
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateUserAsync(userName, email, password, role));

        exception.Message.ShouldContain(errorDescription);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GeneratePasswordResetTokenAsync Tests

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_WhenUserExists_ReturnsToken()
    {
        // Arrange
        var userName = "testuser";
        var expectedToken = "reset-token-123";
        var appUser = new ApplicationUser { UserName = userName };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(appUser))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _sut.GeneratePasswordResetTokenAsync(userName);

        // Assert
        result.ShouldBe(expectedToken);

        _userManagerMock.Verify(
            x => x.FindByNameAsync(userName),
            Times.Once);

        _userManagerMock.Verify(
            x => x.GeneratePasswordResetTokenAsync(appUser),
            Times.Once);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "nonexistent";

        _userManagerMock
            .Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.GeneratePasswordResetTokenAsync(userName));

        exception.Message.ShouldContain(userName);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WhenUserExists_ReturnsNewPassword()
    {
        // Arrange
        var userName = "testuser";
        var appUser = new ApplicationUser { UserName = userName };
        var resetToken = "reset-token";
        var newPassword = "NewPassword123!";

        _userManagerMock
            .Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(appUser))
            .ReturnsAsync(resetToken);

        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(appUser, resetToken, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ResetPasswordAsync(userName);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.Length.ShouldBeGreaterThanOrEqualTo(12);

        _userManagerMock.Verify(
            x => x.FindByNameAsync(userName),
            Times.Once);

        _userManagerMock.Verify(
            x => x.ResetPasswordAsync(appUser, resetToken, It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "nonexistent";

        _userManagerMock
            .Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.ResetPasswordAsync(userName));

        exception.Message.ShouldContain(userName);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenResetFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "testuser";
        var appUser = new ApplicationUser { UserName = userName };
        var errorDescription = "Invalid token";

        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = errorDescription }
        };

        _userManagerMock
            .Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(appUser))
            .ReturnsAsync("reset-token");

        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(appUser, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.ResetPasswordAsync(userName));

        exception.Message.ShouldContain(errorDescription);
    }

    #endregion

    #region BlockUserAsync Tests

    [Fact]
    public async Task BlockUserAsync_WhenUserExists_BlocksUser()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var appUser = new ApplicationUser
        {
            Id = applicationUserId,
            LockoutEnabled = false,
            LockoutEnd = null
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.UpdateAsync(appUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.BlockUserAsync(applicationUserId);

        // Assert
        appUser.LockoutEnabled.ShouldBeTrue();
        appUser.LockoutEnd.ShouldBe(DateTimeOffset.MaxValue);

        _userManagerMock.Verify(
            x => x.FindByIdAsync(applicationUserId.ToString()),
            Times.Once);

        _userManagerMock.Verify(
            x => x.UpdateAsync(appUser),
            Times.Once);
    }

    [Fact]
    public async Task BlockUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.BlockUserAsync(applicationUserId));

        exception.Message.ShouldContain(applicationUserId.ToString());
    }

    [Fact]
    public async Task BlockUserAsync_WhenUpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var appUser = new ApplicationUser { Id = applicationUserId };
        var errorDescription = "Update failed";

        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = errorDescription }
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.UpdateAsync(appUser))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.BlockUserAsync(applicationUserId));

        exception.Message.ShouldContain(errorDescription);
    }

    #endregion

    #region UnblockUserAsync Tests

    [Fact]
    public async Task UnblockUserAsync_WhenUserExists_UnblocksUser()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var appUser = new ApplicationUser
        {
            Id = applicationUserId,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.MaxValue
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.UpdateAsync(appUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.UnblockUserAsync(applicationUserId);

        // Assert
        appUser.LockoutEnabled.ShouldBeFalse();
        appUser.LockoutEnd.ShouldBeNull();

        _userManagerMock.Verify(
            x => x.FindByIdAsync(applicationUserId.ToString()),
            Times.Once);

        _userManagerMock.Verify(
            x => x.UpdateAsync(appUser),
            Times.Once);
    }

    [Fact]
    public async Task UnblockUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.UnblockUserAsync(applicationUserId));

        exception.Message.ShouldContain(applicationUserId.ToString());
    }

    [Fact]
    public async Task UnblockUserAsync_WhenUpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var appUser = new ApplicationUser { Id = applicationUserId };
        var errorDescription = "Update failed";

        var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = errorDescription }
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.UpdateAsync(appUser))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.UnblockUserAsync(applicationUserId));

        exception.Message.ShouldContain(errorDescription);
    }

    #endregion

    #region GetUserRolesAsync Tests

    [Fact]
    public async Task GetUserRolesAsync_WhenUserExists_ReturnsRoles()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var appUser = new ApplicationUser { Id = applicationUserId };
        var expectedRoles = new List<string> { "Manager", "Admin" };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(appUser))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _sut.GetUserRolesAsync(applicationUserId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldContain("Manager");
        result.ShouldContain("Admin");

        _userManagerMock.Verify(
            x => x.FindByIdAsync(applicationUserId.ToString()),
            Times.Once);

        _userManagerMock.Verify(
            x => x.GetRolesAsync(appUser),
            Times.Once);
    }

    [Fact]
    public async Task GetUserRolesAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.GetUserRolesAsync(applicationUserId));

        exception.Message.ShouldContain(applicationUserId.ToString());
    }

    #endregion

    #region SetUserRoleAsync Tests

    [Fact]
    public async Task SetUserRoleAsync_WhenUserExists_SetsRole()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var role = "Manager";
        var appUser = new ApplicationUser { Id = applicationUserId };
        var currentRoles = new List<string> { "OldRole" };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(role))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(appUser))
            .ReturnsAsync(currentRoles);

        _userManagerMock
            .Setup(x => x.RemoveFromRolesAsync(appUser, currentRoles))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(appUser, role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.SetUserRoleAsync(applicationUserId, role);

        // Assert
        _userManagerMock.Verify(
            x => x.FindByIdAsync(applicationUserId.ToString()),
            Times.Once);

        _roleManagerMock.Verify(
            x => x.RoleExistsAsync(role),
            Times.Once);

        _userManagerMock.Verify(
            x => x.RemoveFromRolesAsync(appUser, currentRoles),
            Times.Once);

        _userManagerMock.Verify(
            x => x.AddToRoleAsync(appUser, role),
            Times.Once);
    }

    [Fact]
    public async Task SetUserRoleAsync_WhenRoleDoesNotExist_CreatesRole()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var role = "NewRole";
        var appUser = new ApplicationUser { Id = applicationUserId };
        var currentRoles = new List<string> { "OldRole" };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync(appUser);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(role))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(appUser))
            .ReturnsAsync(currentRoles);

        _userManagerMock
            .Setup(x => x.RemoveFromRolesAsync(appUser, currentRoles))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(appUser, role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.SetUserRoleAsync(applicationUserId, role);

        // Assert
        _roleManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationRole>()),
            Times.Once);
    }

    [Fact]
    public async Task SetUserRoleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var role = "Manager";

        _userManagerMock
            .Setup(x => x.FindByIdAsync(applicationUserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.SetUserRoleAsync(applicationUserId, role));

        exception.Message.ShouldContain(applicationUserId.ToString());
    }

    #endregion

    #region FindApplicationUserByExternalIdAsync Tests

    [Fact]
    public async Task FindApplicationUserByExternalIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();
        var expectedUser = new ApplicationUser { Id = Guid.Parse(externalId), UserName = "testuser" };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(externalId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.FindApplicationUserByExternalIdAsync(externalId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(Guid.Parse(externalId));
        result.UserName.ShouldBe("testuser");

        _userManagerMock.Verify(
            x => x.FindByIdAsync(externalId),
            Times.Once);
    }

    [Fact]
    public async Task FindApplicationUserByExternalIdAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(externalId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.FindApplicationUserByExternalIdAsync(externalId);

        // Assert
        result.ShouldBeNull();

        _userManagerMock.Verify(
            x => x.FindByIdAsync(externalId),
            Times.Once);
    }

    #endregion

    #region GenerateRandomPassword Tests

    [Fact]
    public void GenerateRandomPassword_ReturnsValidPassword()
    {
        // Act
        var password = _sut.GenerateRandomPassword();

        // Assert
        password.ShouldNotBeNullOrEmpty();
        password.Length.ShouldBeGreaterThanOrEqualTo(12);
        password.Length.ShouldBeLessThanOrEqualTo(16);

        password.Any(char.IsLower).ShouldBeTrue();
        password.Any(char.IsUpper).ShouldBeTrue();
        password.Any(char.IsDigit).ShouldBeTrue();
        password.Any(c => !char.IsLetterOrDigit(c)).ShouldBeTrue();
    }

    [Fact]
    public void GenerateRandomPassword_ReturnsDifferentPasswords()
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

    #region Helper Methods

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            store.Object, null, null, null, null);
    }

    #endregion
}