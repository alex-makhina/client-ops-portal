using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class ContractServiceTests
{
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly ContractService _sut;

    public ContractServiceTests()
    {
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _sut = new ContractService(_contractRepositoryMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenContractExists_ReturnsContractDto()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var contract = CreateContractEntity(contractId);

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        var result = await _sut.GetByIdAsync(contractId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(contractId);
        result.ContractNumber.ShouldBe(contract.ContractNumber);

        _contractRepositoryMock.Verify(
            r => r.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenContractNotFound_ReturnsNull()
    {
        // Arrange
        var contractId = Guid.NewGuid();

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        // Act
        var result = await _sut.GetByIdAsync(contractId, true);

        // Assert
        result.ShouldBeNull();

        _contractRepositoryMock.Verify(
            r => r.GetByIdAsync(contractId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludesFalse_PassesParameterToRepository()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var contract = CreateContractEntity(contractId);

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        var result = await _sut.GetByIdAsync(contractId, false);

        // Assert
        result.ShouldNotBeNull();

        _contractRepositoryMock.Verify(
            r => r.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenContractsExist_ReturnsListOfContractDtos()
    {
        // Arrange
        var contracts = CreateContractEntityList(5);
        var expectedCount = contracts.Count;

        _contractRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _contractRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoContractsExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Contract>();

        _contractRepositoryMock
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
    public async Task CreateAsync_WhenValidDto_CreatesContractAndReturnsDto()
    {
        // Arrange
        var createDto = CreateContractDataDto();
        var createdContract = createDto.ToEntity();

        _contractRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
            .Callback<Contract, CancellationToken>((c, ct) =>
            {
                c.Id = createdContract.Id;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.ContractNumber.ShouldBe(createDto.ContractNumber);
        result.AbonentId.ShouldBe(createDto.AbonentId);

        _contractRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Contract>(c =>
                c.ContractNumber == createDto.ContractNumber &&
                c.AbonentId == createDto.AbonentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesContractAndReturnsDto()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var existingContract = CreateContractEntity(contractId);
        var updateDto = CreateUpdateContractDto();

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingContract);

        _contractRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(contractId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(contractId);

        _contractRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenContractNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var updateDto = CreateUpdateContractDto();

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(contractId, updateDto));

        _contractRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEndDateCorrectly()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var existingContract = CreateContractEntity(contractId);
        var updateDto = new UpdateContractDto
        {
            EndDate = DateTimeOffset.UtcNow.AddDays(30)
        };

        _contractRepositoryMock
            .Setup(r => r.GetByIdAsync(contractId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingContract);

        _contractRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(contractId, updateDto);

        // Assert
        result.ShouldNotBeNull();

        _contractRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Contract>(c => c.EndDate == updateDto.EndDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenContractExists_CallsRepositoryDelete()
    {
        // Arrange
        var contractId = Guid.NewGuid();

        _contractRepositoryMock
            .Setup(r => r.DeleteAsync(contractId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(contractId);

        // Assert
        _contractRepositoryMock.Verify(
            r => r.DeleteAsync(contractId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredContracts()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var contracts = CreateContractEntityList(3);
        foreach (var contract in contracts)
        {
            contract.AbonentId = abonentId;
        }

        _contractRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        // Act
        var result = await _sut.GetWhereAsync(c => c.AbonentId == abonentId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(contracts.Count);

        _contractRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<Contract, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Contract>();

        _contractRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetWhereAsync(c => c.AbonentId == Guid.NewGuid(), false);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    #endregion

    #region GetShortContractsByAbonentAsync Tests

    [Fact]
    public async Task GetShortContractsByAbonentAsync_WhenNoContractsExist_ReturnsEmptyList()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var emptyList = new List<Contract>();

        _contractRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _sut.GetShortContractsByAbonentAsync(abonentId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetShortContractsByAbonentAsync_ReturnsOnlyRequiredFields()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var contract = CreateContractEntity(Guid.NewGuid());
        contract.AbonentId = abonentId;
        contract.ContractNumber = "CT-2024-001";
        contract.BeginDate = DateTimeOffset.UtcNow.AddDays(-30);

        _contractRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract> { contract });

        // Act
        var result = await _sut.GetShortContractsByAbonentAsync(abonentId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        var shortDto = result.First();
        shortDto.ContractNumber.ShouldBe(contract.ContractNumber);
        shortDto.AbonentId.ShouldBe(contract.AbonentId);
        shortDto.BeginDate.ShouldBe(contract.BeginDate);
    }

    #endregion

    #region Helper Methods

    private static Contract CreateContractEntity(Guid? id = null)
    {
        var faker = new AutoFaker<Contract>();

        if (id.HasValue)
            faker.RuleFor(c => c.Id, _ => id.Value);

        return faker.Generate();
    }

    private static List<Contract> CreateContractEntityList(int count)
    {
        var list = new List<Contract>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateContractEntity());
        }
        return list;
    }

    private static ContractDataDto CreateContractDataDto()
    {
        return new AutoFaker<ContractDataDto>().Generate();
    }

    private static UpdateContractDto CreateUpdateContractDto()
    {
        return new AutoFaker<UpdateContractDto>().Generate();
    }

    #endregion
}