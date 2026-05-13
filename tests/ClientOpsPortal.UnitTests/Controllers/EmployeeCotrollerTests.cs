using AutoBogus;
using Bogus;
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

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly EmployeesController _sut;

    public EmployeesControllerTests()
    {
        _employeeServiceMock = new Mock<IEmployeeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new EmployeesController(_employeeServiceMock.Object, _currentUserServiceMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenEmployeesExist_ReturnsOkWithEmployees()
    {
        // Arrange
        var expectedEmployees = CreateEmployeeDtoList(5);

        _employeeServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployees);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employees = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeDto>>();
        employees.Count().ShouldBe(5);

        _employeeServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoEmployeesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<EmployeeDto>().AsReadOnly();

        _employeeServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employees = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeDto>>();
        employees.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedEmployees = CreateEmployeeDtoList(3);

        _employeeServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployees);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _employeeServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenEmployeeExists_ReturnsOkWithEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var expectedEmployee = CreateEmployeeDto(employeeId);

        _employeeServiceMock
            .Setup(s => s.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _sut.GetById(employeeId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employee = okResult.Value.ShouldBeOfType<EmployeeDto>();

        employee.Id.ShouldBe(employeeId);
        employee.StaffNumber.ShouldBe(expectedEmployee.StaffNumber);

        _employeeServiceMock.Verify(
            s => s.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenEmployeeNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        _employeeServiceMock
            .Setup(s => s.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeDto?)null);

        // Act
        var result = await _sut.GetById(employeeId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(employeeId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _employeeServiceMock.Verify(
            s => s.GetByIdAsync(employeeId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToService()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var expectedEmployee = CreateEmployeeDto(employeeId);

        _employeeServiceMock
            .Setup(s => s.GetByIdAsync(employeeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _sut.GetById(employeeId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _employeeServiceMock.Verify(
            s => s.GetByIdAsync(employeeId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByStaffNumber Tests

    [Fact]
    public async Task GetByStaffNumber_WhenEmployeeExists_ReturnsOkWithEmployee()
    {
        // Arrange
        var staffNumber = "EMP-2024-001";
        var expectedEmployee = CreateEmployeeShortDataDto(staffNumber: staffNumber);

        _employeeServiceMock
            .Setup(s => s.GetEmployeeByStaffNumberAsync(staffNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployee);

        // Act
        var result = await _sut.GetByStaffNumber(staffNumber);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employee = okResult.Value.ShouldBeOfType<EmployeeShortDataDto>();

        employee.StaffNumber.ShouldBe(staffNumber);

        _employeeServiceMock.Verify(
            s => s.GetEmployeeByStaffNumberAsync(staffNumber, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByStaffNumber_WhenEmployeeNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var staffNumber = "EMP-2024-999";

        _employeeServiceMock
            .Setup(s => s.GetEmployeeByStaffNumberAsync(staffNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeShortDataDto?)null);

        // Act
        var result = await _sut.GetByStaffNumber(staffNumber);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(staffNumber);
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region SearchByName Tests

    [Fact]
    public async Task SearchByName_WithValidSearchTerm_ReturnsOkWithResults()
    {
        // Arrange
        var searchTerm = "Иван";
        var expectedResults = CreateEmployeeShortDataDtoList(3);

        _employeeServiceMock
            .Setup(s => s.SearchByFullNameAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _sut.SearchByName(searchTerm);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var results = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeShortDataDto>>();
        results.Count().ShouldBe(3);

        _employeeServiceMock.Verify(
            s => s.SearchByFullNameAsync(searchTerm, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SearchByName_WithEmptySearchTerm_ReturnsBadRequest(string? searchTerm)
    {
        // Act
        var result = await _sut.SearchByName(searchTerm!);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain("Search term cannot be empty");

        _employeeServiceMock.Verify(
            s => s.SearchByFullNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchByName_WhenNoResults_ReturnsOkWithEmptyList()
    {
        // Arrange
        var searchTerm = "NonExistent";

        _employeeServiceMock
            .Setup(s => s.SearchByFullNameAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeShortDataDto>());

        // Act
        var result = await _sut.SearchByName(searchTerm);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var results = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeShortDataDto>>();
        results.ShouldBeEmpty();
    }

    #endregion

    #region GetByPost Tests

    [Fact]
    public async Task GetByPost_WithValidPost_ReturnsOkWithEmployees()
    {
        // Arrange
        var post = "Менеджер";
        var expectedEmployees = CreateEmployeeShortDataDtoList(4);

        _employeeServiceMock
            .Setup(s => s.GetEmployeesByPostAsync(post, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployees);

        // Act
        var result = await _sut.GetByPost(post);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employees = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeShortDataDto>>();
        employees.Count().ShouldBe(4);

        _employeeServiceMock.Verify(
            s => s.GetEmployeesByPostAsync(post, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetByPost_WithEmptyPost_ReturnsBadRequest(string? post)
    {
        // Act
        var result = await _sut.GetByPost(post!);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain("Post cannot be empty");

        _employeeServiceMock.Verify(
            s => s.GetEmployeesByPostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByPost_WhenNoEmployees_ReturnsOkWithEmptyList()
    {
        // Arrange
        var post = "NonExistent";

        _employeeServiceMock
            .Setup(s => s.GetEmployeesByPostAsync(post, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeShortDataDto>());

        // Act
        var result = await _sut.GetByPost(post);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employees = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeShortDataDto>>();
        employees.ShouldBeEmpty();
    }

    #endregion

    #region GetByDepartment Tests

    [Fact]
    public async Task GetByDepartment_WithValidDepartment_ReturnsOkWithEmployees()
    {
        // Arrange
        var department = "IT";
        var expectedEmployees = CreateEmployeeShortDataDtoList(3);

        _employeeServiceMock
            .Setup(s => s.GetEmployeesByDepartmentAsync(department, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEmployees);

        // Act
        var result = await _sut.GetByDepartment(department);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employees = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeShortDataDto>>();
        employees.Count().ShouldBe(3);

        _employeeServiceMock.Verify(
            s => s.GetEmployeesByDepartmentAsync(department, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetByDepartment_WithEmptyDepartment_ReturnsBadRequest(string? department)
    {
        // Act
        var result = await _sut.GetByDepartment(department!);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain("Department cannot be empty");

        _employeeServiceMock.Verify(
            s => s.GetEmployeesByDepartmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByDepartment_WhenNoEmployees_ReturnsOkWithEmptyList()
    {
        // Arrange
        var department = "NonExistent";

        _employeeServiceMock
            .Setup(s => s.GetEmployeesByDepartmentAsync(department, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeShortDataDto>());

        // Act
        var result = await _sut.GetByDepartment(department);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employees = okResult.Value.ShouldBeAssignableTo<IEnumerable<EmployeeShortDataDto>>();
        employees.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithEmployee()
    {
        // Arrange
        var createDto = CreateCreateEmployeeDto();
        var createdDto = CreateEmployeeDto(id: Guid.NewGuid(), staffNumber: createDto.StaffNumber);

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(EmployeesController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<EmployeeDto>().ShouldBe(createdDto);

        _employeeServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenDuplicateStaffNumber_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateEmployeeDto();
        var errorMessage = $"Сотрудник с табельным номером {createDto.StaffNumber} уже существует";

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _employeeServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenEmployeeExists_ReturnsOkWithUpdatedEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var updateDto = CreateUpdateEmployeeDto();
        var updatedDto = CreateEmployeeDto(employeeId, staffNumber: updateDto.StaffNumber);

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.UpdateAsync(employeeId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(employeeId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var employee = okResult.Value.ShouldBeOfType<EmployeeDto>();

        employee.Id.ShouldBe(employeeId);
        employee.StaffNumber.ShouldBe(updateDto.StaffNumber);

        _employeeServiceMock.Verify(
            s => s.UpdateAsync(employeeId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenEmployeeNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var updateDto = CreateUpdateEmployeeDto();

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.UpdateAsync(employeeId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Employee), employeeId));

        // Act
        var result = await _sut.Update(employeeId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(employeeId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    [Fact]
    public async Task Update_WhenDuplicateStaffNumber_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var updateDto = CreateUpdateEmployeeDto();
        var errorMessage = $"Сотрудник с табельным номером {updateDto.StaffNumber} уже существует";

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.UpdateAsync(employeeId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _sut.Update(employeeId, updateDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenEmployeeExists_ReturnsNoContent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.DeleteAsync(employeeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(employeeId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _employeeServiceMock.Verify(
            s => s.DeleteAsync(employeeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenEmployeeNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        SetupUserRole("Admin");

        _employeeServiceMock
            .Setup(s => s.DeleteAsync(employeeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Employee), employeeId));

        // Act
        var result = await _sut.Delete(employeeId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(employeeId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
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

    private static CreateEmployeeDto CreateCreateEmployeeDto()
    {
        var faker = new Faker();
        return new AutoFaker<CreateEmployeeDto>()
            .RuleFor(dto => dto.StaffNumber, f => $"EMP-{faker.Date.Past(1)}-{faker.Random.Number(1, 9999):D4}")
            .RuleFor(dto => dto.FirstName, f => faker.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => faker.Name.LastName())
            .RuleFor(dto => dto.MiddleName, f => faker.Name.FirstName())
            .RuleFor(dto => dto.Post, f => faker.Name.JobTitle())
            .RuleFor(dto => dto.Department, f => faker.Commerce.Department())
            .RuleFor(dto => dto.Email, f => faker.Internet.Email())
            .Generate();
    }

    private static UpdateEmployeeDto CreateUpdateEmployeeDto()
    {
        var faker = new Faker();
        return new AutoFaker<UpdateEmployeeDto>()
            .RuleFor(dto => dto.StaffNumber, f => $"EMP-{faker.Date.Past(1)}-{faker.Random.Number(1, 9999):D4}")
            .RuleFor(dto => dto.FirstName, f => faker.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => faker.Name.LastName())
            .RuleFor(dto => dto.MiddleName, f => faker.Name.FirstName())
            .RuleFor(dto => dto.Post, f => faker.Name.JobTitle())
            .RuleFor(dto => dto.Department, f => faker.Commerce.Department())
            .Generate();
    }

    private static EmployeeDto CreateEmployeeDto(Guid? id = null, string? staffNumber = null)
    {
        var faker = new Faker();
        var autoFaker = new AutoFaker<EmployeeDto>();

        if (id.HasValue)
            autoFaker.RuleFor(dto => dto.Id, _ => id.Value);
        else
            autoFaker.RuleFor(dto => dto.Id, _ => faker.Random.Guid());

        if (!string.IsNullOrEmpty(staffNumber))
            autoFaker.RuleFor(dto => dto.StaffNumber, _ => staffNumber);
        else
            autoFaker.RuleFor(dto => dto.StaffNumber, _ => $"EMP-{faker.Date.Past(1)}-{faker.Random.Number(1, 9999):D4}");

        return autoFaker
            .RuleFor(dto => dto.UserId, _ => faker.Random.Guid())
            .RuleFor(dto => dto.FirstName, _ => faker.Name.FirstName())
            .RuleFor(dto => dto.LastName, _ => faker.Name.LastName())
            .RuleFor(dto => dto.MiddleName, _ => faker.Name.FirstName())
            .RuleFor(dto => dto.Post, _ => faker.Name.JobTitle())
            .RuleFor(dto => dto.Department, _ => faker.Commerce.Department())
            .RuleFor(dto => dto.CreatedAt, _ => faker.Date.PastOffset())
            .RuleFor(dto => dto.UpdatedAt, _ => faker.Date.RecentOffset())
            .Generate();
    }

    private static List<EmployeeDto> CreateEmployeeDtoList(int count)
    {
        var list = new List<EmployeeDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateEmployeeDto());
        }
        return list;
    }

    private static EmployeeShortDataDto CreateEmployeeShortDataDto(string? staffNumber = null)
    {
        var faker = new Faker();
        return new AutoFaker<EmployeeShortDataDto>()
            .RuleFor(dto => dto.StaffNumber, f => staffNumber== null ? $"EMP-{faker.Date.Past(1)}-{faker.Random.Number(1, 9999):D4}" : staffNumber)
            .RuleFor(dto => dto.FullName, f => $"{faker.Name.LastName()} {faker.Name.FirstName()} {faker.Name.FirstName()}")
            .RuleFor(dto => dto.Post, f => faker.Name.JobTitle())
            .RuleFor(dto => dto.Department, f => faker.Commerce.Department())
            .Generate();
    }

    private static List<EmployeeShortDataDto> CreateEmployeeShortDataDtoList(int count)
    {
        var list = new List<EmployeeShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateEmployeeShortDataDto());
        }
        return list;
    }

    #endregion
}