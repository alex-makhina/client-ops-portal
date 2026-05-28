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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ClientOpsPortal.UnitTests.Controllers;

public class AbonentsControllerTests
{
    private readonly Mock<IAbonentService> _abonentServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AbonentsController _sut;

    public AbonentsControllerTests()
    {
        _abonentServiceMock = new Mock<IAbonentService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new AbonentsController(_abonentServiceMock.Object, _currentUserServiceMock.Object);

        AutoFaker.Configure(builder =>
        {
            builder.WithLocale("ru");
        });
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenAbonentsExist_ReturnsOkWithAbonents()
    {
        // Arrange
        var expectedAbonents = CreateAbonentDtoList(5);

        _abonentServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAbonents);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var abonents = okResult.Value.ShouldBeAssignableTo<IEnumerable<AbonentDto>>();
        abonents.Count().ShouldBe(5);

        _abonentServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoAbonentsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _abonentServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AbonentDto>());

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var abonents = okResult.Value.ShouldBeAssignableTo<IEnumerable<AbonentDto>>();
        abonents.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedAbonents = CreateAbonentDtoList(3);

        _abonentServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAbonents);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _abonentServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenAbonentExists_ReturnsOkWithAbonent()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var expectedAbonent = CreateAbonentDto(abonentId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAbonent);

        // Act
        var result = await _sut.GetById(abonentId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var abonent = okResult.Value.ShouldBeOfType<AbonentDto>();

        abonent.Id.ShouldBe(abonentId);
        abonent.AccountNumber.ShouldBe(expectedAbonent.AccountNumber);

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenAbonentNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var abonentId = Guid.NewGuid();

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AbonentDto?)null);

        // Act
        var result = await _sut.GetById(abonentId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(abonentId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToService()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var expectedAbonent = CreateAbonentDto(abonentId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAbonent);

        // Act
        var result = await _sut.GetById(abonentId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByAccountNumber Tests

    [Fact]
    public async Task GetByAccountNumber_WhenAbonentExists_ReturnsOkWithAbonent()
    {
        // Arrange
        var accountNumber = "1234567890";
        var expectedAbonent = CreateAbonentDto(accountNumber: accountNumber);
        var expectedCollection = new List<AbonentDto> { expectedAbonent }.AsReadOnly();

        _abonentServiceMock
            .Setup(s => s.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCollection);

        // Act
        var result = await _sut.GetByAccountNumber(accountNumber, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var abonent = okResult.Value.ShouldBeOfType<AbonentDto>();
        abonent.AccountNumber.ShouldBe(accountNumber);

        _abonentServiceMock.Verify(
            s => s.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByAccountNumber_WhenAbonentNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var accountNumber = "9999999999";
        var emptyCollection = new List<AbonentDto>().AsReadOnly();

        _abonentServiceMock
            .Setup(s => s.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyCollection);

        // Act
        var result = await _sut.GetByAccountNumber(accountNumber, true);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();

        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.ShouldNotBeNull();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(accountNumber);
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region SearchByName Tests

    [Fact]
    public async Task SearchByName_WithValidSearchTerm_ReturnsOkWithResults()
    {
        // Arrange
        var searchTerm = "Иван";
        var expectedResults = CreateAbonentShortDataDtoList(3);

        _abonentServiceMock
            .Setup(s => s.SearchByFullNameAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _sut.SearchByName(searchTerm);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var results = okResult.Value.ShouldBeAssignableTo<IEnumerable<AbonentShortDataDto>>();
        results.Count().ShouldBe(3);

        _abonentServiceMock.Verify(
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

        _abonentServiceMock.Verify(
            s => s.SearchByFullNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchByName_WhenNoResults_ReturnsOkWithEmptyList()
    {
        // Arrange
        var searchTerm = "NonExistent";

        _abonentServiceMock
            .Setup(s => s.SearchByFullNameAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AbonentShortDataDto>());

        // Act
        var result = await _sut.SearchByName(searchTerm);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var results = okResult.Value.ShouldBeAssignableTo<IEnumerable<AbonentShortDataDto>>();
        results.ShouldBeEmpty();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithAbonent()
    {
        // Arrange
        var createDto = CreateCreateAbonentDto();
        var createdDto = CreateAbonentDto(id: Guid.NewGuid(), accountNumber: createDto.AccountNumber);

        _abonentServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(AbonentsController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<AbonentDto>().ShouldBe(createdDto);

        _abonentServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenDuplicateAccountNumber_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateCreateAbonentDto();
        var errorMessage = $"Абонент с лицевым счетом {createDto.AccountNumber} уже существует";

        _abonentServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _abonentServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenManagerUpdatesExistingAbonent_ReturnsOkWithUpdatedAbonent()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var updateDto = CreateUpdateAbonentDto();
        var updatedDto = CreateAbonentDto(abonentId);

        SetupUserRole("Manager");

        _abonentServiceMock
            .Setup(s => s.UpdateAsync(abonentId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(abonentId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var abonent = okResult.Value.ShouldBeOfType<AbonentDto>();

        abonent.Id.ShouldBe(abonentId);

        _abonentServiceMock.Verify(
            s => s.UpdateAsync(abonentId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_WhenAbonentRoleUpdatesOwnAbonent_ReturnsOkWithUpdatedAbonent()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = CreateUpdateAbonentDto();
        var existingAbonent = CreateAbonentDto(abonentId, userId: userId);
        var updatedDto = CreateAbonentDto(abonentId);

        SetupUserRole("Abonent", userId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        _abonentServiceMock
            .Setup(s => s.UpdateAsync(abonentId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(abonentId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var abonent = okResult.Value.ShouldBeOfType<AbonentDto>();

        abonent.Id.ShouldBe(abonentId);

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);

        _abonentServiceMock.Verify(
            s => s.UpdateAsync(abonentId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenAbonentRoleTriesToUpdateForeignAbonent_ReturnsForbid()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var updateDto = CreateUpdateAbonentDto();
        var existingAbonent = CreateAbonentDto(abonentId, userId: differentUserId);

        SetupUserRole("Abonent", currentUserId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        // Act
        var result = await _sut.Update(abonentId, updateDto);

        // Assert
        result.ShouldBeOfType<ForbidResult>();

        _abonentServiceMock.Verify(
            s => s.UpdateAsync(abonentId, updateDto, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_WhenAbonentNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var updateDto = CreateUpdateAbonentDto();

        SetupUserRole("Manager");

        _abonentServiceMock
            .Setup(s => s.UpdateAsync(abonentId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Abonent), abonentId));

        // Act
        var result = await _sut.Update(abonentId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(abonentId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenAbonentExists_ReturnsNoContent()
    {
        // Arrange
        var abonentId = Guid.NewGuid();

        _abonentServiceMock
            .Setup(s => s.DeleteAsync(abonentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(abonentId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _abonentServiceMock.Verify(
            s => s.DeleteAsync(abonentId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenAbonentNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var abonentId = Guid.NewGuid();

        _abonentServiceMock
            .Setup(s => s.DeleteAsync(abonentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Abonent), abonentId));

        // Act
        var result = await _sut.Delete(abonentId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(abonentId.ToString());
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

    private static CreateAbonentDto CreateCreateAbonentDto()
    {
        return new AutoFaker<CreateAbonentDto>()
            .RuleFor(dto => dto.IdentificationNumber, f => f.Random.Replace("##########"))
            .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => f.Name.LastName())
            .RuleFor(dto => dto.MiddleName, f => f.Name.FirstName())
            .RuleFor(dto => dto.AccountNumber, f => f.Random.Replace("???-##########"))
            .RuleFor(dto => dto.Email, f => f.Internet.Email())
            .Generate();
    }

    private static UpdateAbonentDto CreateUpdateAbonentDto()
    {
        return new AutoFaker<UpdateAbonentDto>()
            .RuleFor(dto => dto.IdentificationNumber, f => f.Random.Replace("##########"))
            .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => f.Name.LastName())
            .RuleFor(dto => dto.MiddleName, f => f.Name.FirstName())
            .Generate();
    }

    private static AbonentDto CreateAbonentDto(
        Guid? id = null,
        string? identificationNumber = null,
        string? firstName = null,
        string? lastName = null,
        string? middleName = null,
        string? accountNumber = null,
        Guid? userId = null)
    {
        var faker = new AutoFaker<AbonentDto>();

        if (id.HasValue)
            faker.RuleFor(dto => dto.Id, _ => id.Value);
        else
            faker.RuleFor(dto => dto.Id, f => f.Random.Guid());

        if (userId.HasValue)
            faker.RuleFor(dto => dto.UserId, _ => userId.Value);
        else
            faker.RuleFor(dto => dto.UserId, f => f.Random.Guid());

        if (!string.IsNullOrEmpty(identificationNumber))
            faker.RuleFor(dto => dto.IdentificationNumber, _ => identificationNumber);

        if (!string.IsNullOrEmpty(firstName))
            faker.RuleFor(dto => dto.FirstName, _ => firstName);

        if (!string.IsNullOrEmpty(lastName))
            faker.RuleFor(dto => dto.LastName, _ => lastName);

        if (!string.IsNullOrEmpty(middleName))
            faker.RuleFor(dto => dto.MiddleName, _ => middleName);

        if (!string.IsNullOrEmpty(accountNumber))
            faker.RuleFor(dto => dto.AccountNumber, _ => accountNumber);

        return faker.Generate();
    }

    private static List<AbonentDto> CreateAbonentDtoList(int count)
    {
        var list = new List<AbonentDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateAbonentDto());
        }
        return list;
    }

    private static AbonentShortDataDto CreateAbonentShortDataDto()
    {
        return new AutoFaker<AbonentShortDataDto>()
            .RuleFor(dto => dto.AccountNumber, f => f.Random.Replace("???-##########"))
            .RuleFor(dto => dto.FullName, f => $"{f.Name.LastName()} {f.Name.FirstName()} {f.Name.FirstName()}")
            .Generate();
    }

    private static List<AbonentShortDataDto> CreateAbonentShortDataDtoList(int count)
    {
        var list = new List<AbonentShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateAbonentShortDataDto());
        }
        return list;
    }

    #endregion
}