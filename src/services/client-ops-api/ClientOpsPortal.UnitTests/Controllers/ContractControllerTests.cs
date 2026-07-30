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
using System.Security.Claims;

namespace ClientOpsPortal.UnitTests.Controllers;

public class ContractsControllerTests
{
    private readonly Mock<IContractService> _contractServiceMock;
    private readonly Mock<IAbonentService> _abonentServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ContractsController _sut;

    public ContractsControllerTests()
    {
        _contractServiceMock = new Mock<IContractService>();
        _abonentServiceMock = new Mock<IAbonentService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new ContractsController(
            _contractServiceMock.Object,
            _abonentServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WhenContractsExist_ReturnsOkWithContracts()
    {
        // Arrange
        var expectedContracts = CreateContractDtoList(5);

        _contractServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContracts);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var contracts = okResult.Value.ShouldBeAssignableTo<IEnumerable<ContractDataDto>>();
        contracts.Count().ShouldBe(5);

        _contractServiceMock.Verify(
            s => s.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoContractsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<ContractDataDto>().AsReadOnly();

        _contractServiceMock
            .Setup(s => s.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetAll(false);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var contracts = okResult.Value.ShouldBeAssignableTo<IEnumerable<ContractDataDto>>();
        contracts.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithIncludes_PassesParameterToService()
    {
        // Arrange
        var expectedContracts = CreateContractDtoList(3);

        _contractServiceMock
            .Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContracts);

        // Act
        var result = await _sut.GetAll(true);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _contractServiceMock.Verify(
            s => s.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenContractExists_ReturnsOkWithContract()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var expectedContract = CreateContractDto(contractId);

        _contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContract);

        // Act
        var result = await _sut.GetById(contractId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var contract = okResult.Value.ShouldBeOfType<ContractDataDto>();

        contract.Id.ShouldBe(contractId);
        contract.ContractNumber.ShouldBe(expectedContract.ContractNumber);

        _contractServiceMock.Verify(
            s => s.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenContractNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var contractId = Guid.NewGuid();

        _contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractDataDto?)null);

        // Act
        var result = await _sut.GetById(contractId, true);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(contractId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");

        _contractServiceMock.Verify(
            s => s.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithIncludesFalse_PassesParameterToService()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var expectedContract = CreateContractDto(contractId);

        _contractServiceMock
            .Setup(s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContract);

        // Act
        var result = await _sut.GetById(contractId, false);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();

        _contractServiceMock.Verify(
            s => s.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByAbonent Tests

    [Fact]
    public async Task GetByAbonent_WhenManager_ReturnsOkWithContracts()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var expectedContracts = CreateContractShortDataDtoList(3);

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.GetShortContractsByAbonentAsync(abonentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContracts);

        // Act
        var result = await _sut.GetByAbonent(abonentId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var contracts = okResult.Value.ShouldBeAssignableTo<IEnumerable<ContractShortDataDto>>();
        contracts.Count().ShouldBe(3);

        _contractServiceMock.Verify(
            s => s.GetShortContractsByAbonentAsync(abonentId, It.IsAny<CancellationToken>()),
            Times.Once);

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByAbonent_WhenAbonentRoleAndOwnsAbonent_ReturnsOkWithContracts()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedContracts = CreateContractShortDataDtoList(2);
        var existingAbonent = CreateAbonentDto(abonentId, userId: userId);

        SetupUserRole("Abonent", userId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        _contractServiceMock
            .Setup(s => s.GetShortContractsByAbonentAsync(abonentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContracts);

        // Act
        var result = await _sut.GetByAbonent(abonentId, true);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var contracts = okResult.Value.ShouldBeAssignableTo<IEnumerable<ContractShortDataDto>>();
        contracts.Count().ShouldBe(2);

        _abonentServiceMock.Verify(
            s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()),
            Times.Once);

        _contractServiceMock.Verify(
            s => s.GetShortContractsByAbonentAsync(abonentId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByAbonent_WhenAbonentRoleAndDoesNotOwnAbonent_ReturnsForbid()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var existingAbonent = CreateAbonentDto(abonentId, userId: differentUserId);

        SetupUserRole("Abonent", currentUserId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        // Act
        var result = await _sut.GetByAbonent(abonentId, true);

        // Assert
        result.ShouldBeOfType<ForbidResult>();

        _contractServiceMock.Verify(
            s => s.GetShortContractsByAbonentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByAbonent_WhenAbonentRoleAndAbonentNotFound_ReturnsForbid()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupUserRole("Abonent", userId);

        _abonentServiceMock
            .Setup(s => s.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AbonentDto?)null);

        // Act
        var result = await _sut.GetByAbonent(abonentId, true);

        // Assert
        result.ShouldBeOfType<ForbidResult>();

        _contractServiceMock.Verify(
            s => s.GetShortContractsByAbonentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedAtActionWithContract()
    {
        // Arrange
        var createDto = CreateContractDataDto();
        var createdDto = CreateContractDto(id: Guid.NewGuid(), contractNumber: createDto.ContractNumber);

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(ContractsController.GetById));
        createdResult.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues["id"].ShouldBe(createdDto.Id);
        createdResult.Value.ShouldBeOfType<ContractDataDto>().ShouldBe(createdDto);

        _contractServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenDuplicateNumber_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var createDto = CreateContractDataDto();
        var errorMessage = $"Договор с номером {createDto.ContractNumber} уже существует";

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldNotBeNull();
        badRequestResult.Value.ToString().ShouldContain(errorMessage);

        _contractServiceMock.Verify(
            s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenContractExists_ReturnsOkWithUpdatedContract()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var updateDto = CreateUpdateContractDto();
        var updatedDto = CreateContractDto(contractId);

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.UpdateAsync(contractId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.Update(contractId, updateDto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var contract = okResult.Value.ShouldBeOfType<ContractDataDto>();

        contract.Id.ShouldBe(contractId);

        _contractServiceMock.Verify(
            s => s.UpdateAsync(contractId, updateDto, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenContractNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var updateDto = CreateUpdateContractDto();

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.UpdateAsync(contractId, updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Contract), contractId));

        // Act
        var result = await _sut.Update(contractId, updateDto);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(contractId.ToString());
        notFoundResult.Value.ToString().ShouldContain("не найден");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenContractExists_ReturnsNoContent()
    {
        // Arrange
        var contractId = Guid.NewGuid();

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.DeleteAsync(contractId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(contractId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        _contractServiceMock.Verify(
            s => s.DeleteAsync(contractId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenContractNotFound_ReturnsNotFoundWithMessage()
    {
        // Arrange
        var contractId = Guid.NewGuid();

        SetupUserRole("Manager");

        _contractServiceMock
            .Setup(s => s.DeleteAsync(contractId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException(typeof(Contract), contractId));

        // Act
        var result = await _sut.Delete(contractId);

        // Assert
        var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ToString().ShouldContain(contractId.ToString());
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

    private static ContractDataDto CreateContractDataDto()
    {
        return new AutoFaker<ContractDataDto>()
            .RuleFor(dto => dto.ContractNumber, f => $"CT-{f.Date.Past(1).Year}-{f.Random.Number(1, 9999):D4}")
            .RuleFor(dto => dto.AbonentId, f => f.Random.Guid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(dto => dto.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();
    }

    private static UpdateContractDto CreateUpdateContractDto()
    {
        return new AutoFaker<UpdateContractDto>()
            .RuleFor(dto => dto.EndDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(dto => dto.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();
    }

    private static ContractDataDto CreateContractDto(Guid? id = null, string? contractNumber = null)
    {
        var faker = new AutoFaker<ContractDataDto>();

        if (id.HasValue)
            faker.RuleFor(dto => dto.Id, _ => id.Value);
        else
            faker.RuleFor(dto => dto.Id, f => f.Random.Guid());

        if (!string.IsNullOrEmpty(contractNumber))
            faker.RuleFor(dto => dto.ContractNumber, _ => contractNumber);

        return faker
            .RuleFor(dto => dto.AbonentId, f => f.Random.Guid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(dto => dto.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();
    }

    private static List<ContractDataDto> CreateContractDtoList(int count)
    {
        var list = new List<ContractDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateContractDto());
        }
        return list;
    }

    private static ContractShortDataDto CreateContractShortDataDto()
    {
        return new AutoFaker<ContractShortDataDto>()
            .RuleFor(dto => dto.ContractNumber, f => $"CT-{f.Date.Past(1).Year}-{f.Random.Number(1, 9999):D4}")
            .RuleFor(dto => dto.AbonentId, f => f.Random.Guid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .Generate();
    }

    private static List<ContractShortDataDto> CreateContractShortDataDtoList(int count)
    {
        var list = new List<ContractShortDataDto>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateContractShortDataDto());
        }
        return list;
    }

    private static AbonentDto CreateAbonentDto(Guid? id = null, Guid? userId = null)
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

        return faker
            .RuleFor(dto => dto.IdentificationNumber, f => f.Random.Replace("##########"))
            .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => f.Name.LastName())
            .RuleFor(dto => dto.MiddleName, f => f.Name.FirstName())
            .RuleFor(dto => dto.AccountNumber, f => f.Random.Replace("???-##########"))
            .Generate();
    }

    #endregion
}