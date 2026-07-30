using AutoBogus;
using ClientOpsPortal.Services.Auth.Controllers;
using ClientOpsPortal.Services.Auth.Contracts;
using ClientOpsPortal.Services.Auth.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace ClientOpsPortal.Services.Auth.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();
        _sut = new UsersController(_userManagerMock.Object, _roleManagerMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenUsersExist_ReturnsListOfUserResponses()
    {
        // Arrange
        var users = CreateApplicationUserList(3);
        var roles = new List<string> { "Manager" };

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(users.AsQueryable());

        foreach (var user in users)
        {
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);
        }

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var responses = okResult.Value.ShouldBeOfType<List<UserResponse>>();

        responses.Count.ShouldBe(3);
        foreach (var response in responses)
        {
            response.UserName.ShouldNotBeEmpty();
            response.Email.ShouldNotBeEmpty();
            response.Roles.ShouldBe(roles);
            response.IsBlocked.ShouldBeFalse();
        }

        _userManagerMock.Verify(x => x.Users, Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoUsersExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<ApplicationUser>();

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(emptyList.AsQueryable());

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var responses = okResult.Value.ShouldBeOfType<List<UserResponse>>();
        responses.ShouldBeEmpty();

        _userManagerMock.Verify(x => x.Users, Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenUsersHaveDifferentRoles_ReturnsCorrectRoles()
    {
        // Arrange
        var users = CreateApplicationUserList(2);
        var roles1 = new List<string> { "Admin", "Manager" };
        var roles2 = new List<string> { "Abonent" };

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(users.AsQueryable());

        _userManagerMock
            .Setup(x => x.GetRolesAsync(users[0]))
            .ReturnsAsync(roles1);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(users[1]))
            .ReturnsAsync(roles2);

        _userManagerMock
            .Setup(x => x.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var responses = okResult.Value.ShouldBeOfType<List<UserResponse>>();

        responses[0].Roles.ShouldBe(roles1);
        responses[1].Roles.ShouldBe(roles2);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenUserExists_ReturnsUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateApplicationUser(userId);
        var roles = new List<string> { "Manager" };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _userManagerMock
            .Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetById(userId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<UserResponse>();

        response.Id.ShouldBe(userId.ToString());
        response.UserName.ShouldBe(user.UserName);
        response.Email.ShouldBe(user.Email);
        response.Roles.ShouldBe(roles);
        response.IsBlocked.ShouldBeFalse();

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
        _userManagerMock.Verify(x => x.IsLockedOutAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.GetById(userId);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _userManagerMock.Verify(x => x.IsLockedOutAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidRequest_CreatesUserAndReturnsUserId()
    {
        // Arrange
        var createRequest = CreateCreateUserRequest();
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = createRequest.UserName,
            Email = createRequest.Email,
            EmailConfirmed = true
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), createRequest.Password))
            .Callback<ApplicationUser, string>((u, _) => u.Id = userId)
            .ReturnsAsync(IdentityResult.Success);

        foreach (var role in createRequest.Roles)
        {
            _roleManagerMock
                .Setup(x => x.RoleExistsAsync(role))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
                .ReturnsAsync(IdentityResult.Success);
        }

        // Act
        var result = await _sut.Create(createRequest);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedId = okResult.Value.ShouldBeOfType<string>();
        returnedId.ShouldBe(userId.ToString());

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), createRequest.Password), Times.Once);
        _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Exactly(createRequest.Roles.Count));
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Exactly(createRequest.Roles.Count));
    }

    [Fact]
    public async Task Create_WhenRoleDoesNotExist_SkipsAddingUserToRole()
    {
        // Arrange
        var createRequest = CreateCreateUserRequest();
        var userId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), createRequest.Password))
            .Callback<ApplicationUser, string>((u, _) => u.Id = userId)
            .ReturnsAsync(IdentityResult.Success);

        foreach (var role in createRequest.Roles)
        {
            _roleManagerMock
                .Setup(x => x.RoleExistsAsync(role))
                .ReturnsAsync(false);
        }

        // Act
        var result = await _sut.Create(createRequest);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenUserCreationFails_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = CreateCreateUserRequest();
        var errors = new[] { new IdentityError { Description = "Username already taken" } };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), createRequest.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _sut.Create(createRequest);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), createRequest.Password), Times.Once);
        _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region SetRole Tests

    [Fact]
    public async Task SetRole_WhenUserExists_UpdatesUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateApplicationUser(userId);
        var currentRoles = new List<string> { "OldRole" };
        var newRole = "NewRole";

        var setRoleRequest = new SetUserRoleRequest
        {
            UserId = userId.ToString(),
            Role = newRole
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        _userManagerMock
            .Setup(x => x.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(newRole))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(user, newRole))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.SetRole(userId, setRoleRequest);

        // Assert
        result.ShouldBeOfType<OkResult>();

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, currentRoles), Times.Once);
        _roleManagerMock.Verify(x => x.RoleExistsAsync(newRole), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, newRole), Times.Once);
    }

    [Fact]
    public async Task SetRole_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var setRoleRequest = new SetUserRoleRequest
        {
            UserId = userId.ToString(),
            Role = "NewRole"
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.SetRole(userId, setRoleRequest);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task SetRole_WhenNewRoleDoesNotExist_DoesNotAddRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateApplicationUser(userId);
        var currentRoles = new List<string> { "OldRole" };
        var newRole = "NonExistentRole";

        var setRoleRequest = new SetUserRoleRequest
        {
            UserId = userId.ToString(),
            Role = newRole
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        _userManagerMock
            .Setup(x => x.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(newRole))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.SetRole(userId, setRoleRequest);

        // Assert
        result.ShouldBeOfType<OkResult>();

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), newRole), Times.Never);
    }

    #endregion

    #region Block Tests

    [Fact]
    public async Task Block_WhenUserExists_BlocksUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateApplicationUser(userId);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.Block(userId);

        // Assert
        result.ShouldBeOfType<OkResult>();

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(x => x.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue), Times.Once);
    }

    [Fact]
    public async Task Block_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.Block(userId);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();

        _userManagerMock.Verify(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset>()), Times.Never);
    }

    #endregion

    #region Unblock Tests

    [Fact]
    public async Task Unblock_WhenUserExists_UnblocksUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateApplicationUser(userId);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.SetLockoutEndDateAsync(user, null))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.ResetAccessFailedCountAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.Unblock(userId);

        // Assert
        result.ShouldBeOfType<OkResult>();

        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(x => x.SetLockoutEndDateAsync(user, null), Times.Once);
        _userManagerMock.Verify(x => x.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task Unblock_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.Unblock(userId);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();

        _userManagerMock.Verify(x => x.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()), Times.Never);
        _userManagerMock.Verify(x => x.ResetAccessFailedCountAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    #endregion

    #region GenerateRandomPassword Tests

    [Fact]
    public void GenerateRandomPassword_ReturnsPasswordWithValidLength()
    {
        // Act
        var result = _sut.GenerateRandomPassword();
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var password = okResult.Value.ShouldBeOfType<string>();

        // Assert
        password.ShouldNotBeNullOrEmpty();
        password.Length.ShouldBe(12);
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneLowercaseLetter()
    {
        // Act
        var result = _sut.GenerateRandomPassword();
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var password = okResult.Value.ShouldBeOfType<string>();

        // Assert
        password.ShouldContain(c => char.IsLower(c));
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneUppercaseLetter()
    {
        // Act
        var result = _sut.GenerateRandomPassword();
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var password = okResult.Value.ShouldBeOfType<string>();

        // Assert
        password.ShouldContain(c => char.IsUpper(c));
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneDigit()
    {
        // Act
        var result = _sut.GenerateRandomPassword();
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var password = okResult.Value.ShouldBeOfType<string>();

        // Assert
        password.ShouldContain(c => char.IsDigit(c));
    }

    [Fact]
    public void GenerateRandomPassword_ContainsAtLeastOneSpecialCharacter()
    {
        // Act
        var result = _sut.GenerateRandomPassword();
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var password = okResult.Value.ShouldBeOfType<string>();
        var specialChars = "!@#$%^&*()-_=+";

        // Assert
        password.ShouldContain(c => specialChars.Contains(c));
    }

    [Fact]
    public void GenerateRandomPassword_ReturnsDifferentPasswordsOnMultipleCalls()
    {
        // Act
        var result1 = _sut.GenerateRandomPassword();
        var result2 = _sut.GenerateRandomPassword();
        var result3 = _sut.GenerateRandomPassword();

        var okResult1 = result1.ShouldBeOfType<OkObjectResult>();
        var okResult2 = result2.ShouldBeOfType<OkObjectResult>();
        var okResult3 = result3.ShouldBeOfType<OkObjectResult>();

        var password1 = okResult1.Value.ShouldBeOfType<string>();
        var password2 = okResult2.Value.ShouldBeOfType<string>();
        var password3 = okResult3.Value.ShouldBeOfType<string>();

        // Assert
        password1.ShouldNotBe(password2);
        password1.ShouldNotBe(password3);
        password2.ShouldNotBe(password3);
    }

    #endregion

    #region Helper Methods

    private static ApplicationUser CreateApplicationUser(Guid? id = null, string? userName = null)
    {
        var userId = id ?? Guid.NewGuid();
        return new AutoFaker<ApplicationUser>()
            .RuleFor(u => u.Id, _ => userId)
            .RuleFor(u => u.UserName, f => userName ?? f.Person.UserName)
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.EmailConfirmed, _ => true)
            .Generate();
    }

    private static List<ApplicationUser> CreateApplicationUserList(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateApplicationUser())
            .ToList();
    }

    private static CreateUserRequest CreateCreateUserRequest()
    {
        return new AutoFaker<CreateUserRequest>()
            .RuleFor(r => r.UserName, f => f.Person.UserName)
            .RuleFor(r => r.Password, f => "Test123!")
            .RuleFor(r => r.Email, f => f.Person.Email)
            .RuleFor(r => r.Roles, f => new List<string> { "Abonent", "Manager" })
            .Generate();
    }

    #endregion
}