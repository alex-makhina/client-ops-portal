using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Contracts.Events;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Services.Auth.Client;
using ClientOpsPortal.Services.Auth.Contracts;
using ClientOpsPortal.Services.Notifications.Client;
using ClientOpsPortal.Services.Notifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Moq;
using Shouldly;
using System.Linq.Expressions;
using Xunit;

namespace ClientOpsPortal.UnitTests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IGenericRepository<Employee>> _employeeRepositoryMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly Mock<IAuthClient> _authClientMock;
    private readonly Mock<INotificationPublisher> _notificationPublisherMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _employeeRepositoryMock = new Mock<IGenericRepository<Employee>>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();
        _authClientMock = new Mock<IAuthClient>();
        _notificationPublisherMock = new Mock<INotificationPublisher>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });

        _authClientMock
            .Setup(x => x.GenerateRandomPasswordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Temp123!");

        _authClientMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid().ToString());

        _authClientMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("reset-token-123");

        _authClientMock
            .Setup(x => x.GetUserByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken ct) => new UserResponse
            {
                Id = id,
                UserName = $"user_{id}",
                Email = $"user_{id}@test.com",
                Roles = new List<string> { "Manager" }
            });

        _authClientMock
            .Setup(x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authClientMock
            .Setup(x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authClientMock
            .Setup(x => x.SetUserRoleAsync(It.IsAny<SetUserRoleRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Настройка Publish для конкретных типов событий
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<EmployeeCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<EmployeeDeletedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Создаем реальную конфигурацию с нужными значениями
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AuthService:PublicUrl", "http://localhost:5110" }
            })
            .Build();

        _sut = new EmployeeService(
            _employeeRepositoryMock.Object,
            _userRepositoryMock.Object,
            _authClientMock.Object,
            _notificationPublisherMock.Object,
            _publishEndpointMock.Object,
            config);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsEmployeeDto()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = CreateEmployeeEntity(employeeId);

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _sut.GetByIdAsync(employeeId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(employeeId);
        result.StaffNumber.ShouldBe(employee.StaffNumber);

        _employeeRepositoryMock.Verify(
            r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeNotFound_ReturnsNull()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _sut.GetByIdAsync(employeeId, true);

        // Assert
        result.ShouldBeNull();

        _employeeRepositoryMock.Verify(
            r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenEmployeesExist_ReturnsListOfEmployeeDtos()
    {
        // Arrange
        var employees = CreateEmployeeEntityList(5);
        var expectedCount = employees.Count;

        _employeeRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _employeeRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoEmployeesExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Employee>();

        _employeeRepositoryMock
            .Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAllAsync(false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidDto_CreatesEmployeeAndReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateEmployeeDto();
        var userId = Guid.NewGuid().ToString();
        var generatedPassword = "Temp123!";

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        _authClientMock
            .Setup(s => s.GenerateRandomPasswordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedPassword);

        _authClientMock
            .Setup(s => s.CreateUserAsync(
                It.Is<CreateUserRequest>(r =>
                    r.UserName == createDto.StaffNumber &&
                    r.Email == createDto.Email &&
                    r.Password == generatedPassword),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _authClientMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("reset-token-123");

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _employeeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.StaffNumber.ShouldBe(createDto.StaffNumber);

        _authClientMock.Verify(
            s => s.CreateUserAsync(
                It.Is<CreateUserRequest>(r =>
                    r.UserName == createDto.StaffNumber &&
                    r.Email == createDto.Email),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _employeeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenStaffNumberNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = CreateCreateEmployeeDto();
        var existingEmployee = CreateEmployeeEntity(Guid.NewGuid());
        existingEmployee.StaffNumber = createDto.StaffNumber;

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { existingEmployee });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateAsync(createDto));

        exception.Message.ShouldContain(createDto.StaffNumber);

        _authClientMock.Verify(
            s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _employeeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesEmployeeAndReturnsDto()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var existingEmployee = CreateEmployeeEntity(employeeId);
        var updateDto = CreateUpdateEmployeeDto();

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        _employeeRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(employeeId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(employeeId);

        _employeeRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var updateDto = CreateUpdateEmployeeDto();

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(employeeId, updateDto));

        _employeeRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenStaffNumberChangedAndNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var existingEmployee = CreateEmployeeEntity(employeeId);
        var updateDto = CreateUpdateEmployeeDto();
        var otherEmployee = CreateEmployeeEntity(Guid.NewGuid());
        otherEmployee.StaffNumber = updateDto.StaffNumber;

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { otherEmployee });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.UpdateAsync(employeeId, updateDto));

        exception.Message.ShouldContain(updateDto.StaffNumber);

        _employeeRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenEmployeeExists_CallsRepositoryDelete()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        _employeeRepositoryMock
            .Setup(r => r.DeleteAsync(employeeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(employeeId);

        // Assert
        _employeeRepositoryMock.Verify(
            r => r.DeleteAsync(employeeId, It.IsAny<CancellationToken>()),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeDeletedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredEmployees()
    {
        // Arrange
        var employees = CreateEmployeeEntityList(3);

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _sut.GetWhereAsync(e => e.Id == employees.First().Id, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<Employee, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetEmployeeByStaffNumberAsync Tests

    [Fact]
    public async Task GetEmployeeByStaffNumberAsync_WhenEmployeeExists_ReturnsShortDto()
    {
        // Arrange
        var staffNumber = "EMP-2024-001";
        var employee = CreateEmployeeEntity(Guid.NewGuid());
        employee.StaffNumber = staffNumber;

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { employee });

        // Act
        var result = await _sut.GetEmployeeByStaffNumberAsync(staffNumber);

        // Assert
        result.ShouldNotBeNull();
        result.StaffNumber.ShouldBe(staffNumber);

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEmployeeByStaffNumberAsync_WhenEmployeeNotFound_ReturnsNull()
    {
        // Arrange
        var staffNumber = "EMP-2024-999";

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        // Act
        var result = await _sut.GetEmployeeByStaffNumberAsync(staffNumber);

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region SearchByFullNameAsync Tests

    [Fact]
    public async Task SearchByFullNameAsync_WithValidSearchTerm_ReturnsMatchedEmployees()
    {
        // Arrange
        var searchTerm = "Иван";
        var employees = CreateEmployeeEntityList(3);

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _sut.SearchByFullNameAsync(searchTerm);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SearchByFullNameAsync_WithEmptySearchTerm_ReturnsEmptyList(string? searchTerm)
    {
        // Act
        var result = await _sut.SearchByFullNameAsync(searchTerm!);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GetEmployeesByPostAsync Tests

    [Fact]
    public async Task GetEmployeesByPostAsync_WithValidPost_ReturnsEmployees()
    {
        // Arrange
        var post = "Менеджер";
        var employees = CreateEmployeeEntityList(3);
        foreach (var employee in employees)
        {
            employee.Post = post;
        }

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _sut.GetEmployeesByPostAsync(post);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetEmployeesByPostAsync_WithEmptyPost_ReturnsEmptyList(string? post)
    {
        // Act
        var result = await _sut.GetEmployeesByPostAsync(post!);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEmployeesByPostAsync_WhenNoEmployeesFound_ReturnsEmptyList()
    {
        // Arrange
        var post = "NonExistentPost";

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        // Act
        var result = await _sut.GetEmployeesByPostAsync(post);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetEmployeesByDepartmentAsync Tests

    [Fact]
    public async Task GetEmployeesByDepartmentAsync_WithValidDepartment_ReturnsEmployees()
    {
        // Arrange
        var department = "IT";
        var employees = CreateEmployeeEntityList(3);
        foreach (var employee in employees)
        {
            employee.Department = department;
        }

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _sut.GetEmployeesByDepartmentAsync(department);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetEmployeesByDepartmentAsync_WithEmptyDepartment_ReturnsEmptyList(string? department)
    {
        // Act
        var result = await _sut.GetEmployeesByDepartmentAsync(department!);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEmployeesByDepartmentAsync_WhenNoEmployeesFound_ReturnsEmptyList()
    {
        // Arrange
        var department = "NonExistentDepartment";

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        // Act
        var result = await _sut.GetEmployeesByDepartmentAsync(department);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_WhenEmployeesExist_ReturnsUserListItems()
    {
        // Arrange
        var employees = CreateEmployeeEntityList(3);
        foreach (var employee in employees)
        {
            employee.User = new User { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString() };
        }

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(employees.Count);

        _employeeRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<Employee, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenAuthClientThrowsException_SkipsUser()
    {
        // Arrange
        var employees = CreateEmployeeEntityList(3);
        foreach (var employee in employees)
        {
            employee.User = new User { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString() };
        }

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        _authClientMock
            .Setup(x => x.GetUserByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Auth service error"));

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _authClientMock.Verify(
            x => x.GetUserByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(employees.Count));
    }
    #endregion

    #region ToggleUserStatusAsync Tests

    [Fact]
    public async Task ToggleUserStatusAsync_WhenEmployeeActive_TogglesToInactive()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = CreateEmployeeEntity(employeeId);
        employee.IsActive = true;
        employee.User = new User { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString() };

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _employeeRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authClientMock
            .Setup(x => x.BlockUserAsync(employee.User.ExternalId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ToggleUserStatusAsync(employeeId);

        // Assert
        employee.IsActive.ShouldBeFalse();

        _authClientMock.Verify(
            x => x.BlockUserAsync(employee.User.ExternalId, It.IsAny<CancellationToken>()),
            Times.Once);

        _authClientMock.Verify(
            x => x.UnblockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_WhenEmployeeInactive_TogglesToActive()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = CreateEmployeeEntity(employeeId);
        employee.IsActive = false;
        employee.User = new User { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString() };

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _employeeRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authClientMock
            .Setup(x => x.UnblockUserAsync(employee.User.ExternalId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ToggleUserStatusAsync(employeeId);

        // Assert
        employee.IsActive.ShouldBeTrue();

        _authClientMock.Verify(
            x => x.UnblockUserAsync(employee.User.ExternalId, It.IsAny<CancellationToken>()),
            Times.Once);

        _authClientMock.Verify(
            x => x.BlockUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_WhenEmployeeNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.ToggleUserStatusAsync(employeeId));

        _employeeRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GetAvailableRolesAsync Tests

    [Fact]
    public async Task GetAvailableRolesAsync_ReturnsListOfRoles()
    {
        // Act
        var result = await _sut.GetAvailableRolesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);
        result.ShouldContain("Администратор");
        result.ShouldContain("Специалист по услугам");
        result.ShouldContain("Аналитик");
        result.ShouldContain("Специалист по работе с клиентами");
    }

    #endregion

    #region CreateAdminUserAsync Tests

    [Fact]
    public async Task CreateAdminUserAsync_WhenValidDto_CreatesAdminUser()
    {
        // Arrange
        var createDto = CreateCreateEmployeeDto();
        createDto.Login = "admin_user";
        createDto.Role = "Администратор";
        createDto.Password = "Admin123!";
        var userId = Guid.NewGuid().ToString();

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        _authClientMock
            .Setup(s => s.CreateUserAsync(
                It.Is<CreateUserRequest>(r =>
                    r.UserName == createDto.Login &&
                    r.Email == createDto.Email &&
                    r.Password == createDto.Password),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _authClientMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("reset-token-123");

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _employeeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAdminUserAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.StaffNumber.ShouldBe(createDto.StaffNumber);

        _authClientMock.Verify(
            s => s.CreateUserAsync(
                It.Is<CreateUserRequest>(r =>
                    r.UserName == createDto.Login &&
                    r.Email == createDto.Email &&
                    r.Roles.Contains("Admin")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _employeeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAdminUserAsync_WhenStaffNumberExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = CreateCreateEmployeeDto();
        var existingEmployee = CreateEmployeeEntity(Guid.NewGuid());
        existingEmployee.StaffNumber = createDto.StaffNumber;

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { existingEmployee });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateAdminUserAsync(createDto));

        exception.Message.ShouldContain(createDto.StaffNumber);

        _authClientMock.Verify(
            s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region UpdateAdminUserAsync Tests

    [Fact]
    public async Task UpdateAdminUserAsync_WhenValidDto_UpdatesAdminUser()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var existingEmployee = CreateEmployeeEntity(employeeId);
        existingEmployee.User = new User { Id = Guid.NewGuid(), ExternalId = Guid.NewGuid().ToString() };
        var updateDto = CreateUpdateEmployeeDto();
        updateDto.Role = "Администратор";

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEmployee);

        _employeeRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Employee, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee>());

        _employeeRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authClientMock
            .Setup(x => x.SetUserRoleAsync(
                It.IsAny<SetUserRoleRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAdminUserAsync(employeeId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(employeeId);

        _authClientMock.Verify(
            x => x.SetUserRoleAsync(
                It.Is<SetUserRoleRequest>(r => r.Role == "Admin"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _employeeRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAdminUserAsync_WhenEmployeeNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var updateDto = CreateUpdateEmployeeDto();

        _employeeRepositoryMock
            .Setup(r => r.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAdminUserAsync(employeeId, updateDto));
    }

    #endregion

    #region Helper Methods

    private static Employee CreateEmployeeEntity(Guid? id = null)
    {
        var faker = new AutoFaker<Employee>();

        if (id.HasValue)
            faker.RuleFor(e => e.Id, _ => id.Value);

        return faker
            .RuleFor(e => e.StaffNumber, _ => $"EMP-{DateTime.Now:yyyy}-{Guid.NewGuid():N}".Substring(0, 15))
            .RuleFor(e => e.FirstName, _ => $"FirstName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(e => e.LastName, _ => $"LastName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(e => e.MiddleName, _ => $"MiddleName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(e => e.Post, _ => "Менеджер")
            .RuleFor(e => e.Department, _ => "IT")
            .RuleFor(e => e.IsActive, _ => true)
            .RuleFor(e => e.User, _ => null)
            .Generate();
    }

    private static List<Employee> CreateEmployeeEntityList(int count)
    {
        var list = new List<Employee>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateEmployeeEntity());
        }
        return list;
    }

    private static CreateEmployeeDto CreateCreateEmployeeDto()
    {
        var faker = new AutoFaker<CreateEmployeeDto>();

        return faker
            .RuleFor(dto => dto.StaffNumber, _ => $"EMP-{DateTime.Now:yyyy}-{Guid.NewGuid():N}".Substring(0, 15))
            .RuleFor(dto => dto.Email, _ => $"{Guid.NewGuid():N}@test.com")
            .RuleFor(dto => dto.FirstName, _ => $"FirstName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(dto => dto.LastName, _ => $"LastName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(dto => dto.MiddleName, _ => $"MiddleName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(dto => dto.Post, _ => "Менеджер")
            .RuleFor(dto => dto.Department, _ => "IT")
            .RuleFor(dto => dto.Role, _ => "Специалист по работе с клиентами")
            .Generate();
    }

    private static UpdateEmployeeDto CreateUpdateEmployeeDto()
    {
        var faker = new AutoFaker<UpdateEmployeeDto>();

        return faker
            .RuleFor(dto => dto.StaffNumber, _ => $"EMP-{DateTime.Now:yyyy}-{Guid.NewGuid():N}".Substring(0, 15))
            .RuleFor(dto => dto.FirstName, _ => $"FirstName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(dto => dto.LastName, _ => $"LastName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(dto => dto.MiddleName, _ => $"MiddleName_{Guid.NewGuid():N}".Substring(0, 10))
            .RuleFor(dto => dto.Post, _ => "Менеджер")
            .RuleFor(dto => dto.Department, _ => "IT")
            .RuleFor(dto => dto.Role, _ => "Специалист по работе с клиентами")
            .Generate();
    }

    #endregion
}