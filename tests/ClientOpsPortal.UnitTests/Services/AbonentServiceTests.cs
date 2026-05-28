using AutoBogus;
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace ClientOpsPortal.UnitTests.Services;

public class AbonentServiceTests
{
    private readonly Mock<IGenericRepository<Abonent>> _abonentRepositoryMock;
    private readonly Mock<IGenericRepository<Contract>> _contractRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AbonentService _sut;

    public AbonentServiceTests()
    {
        _abonentRepositoryMock = new Mock<IGenericRepository<Abonent>>();
        _contractRepositoryMock = new Mock<IGenericRepository<Contract>>();
        _identityServiceMock = new Mock<IIdentityService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _sut = new AbonentService(
            _abonentRepositoryMock.Object,
            _contractRepositoryMock.Object,
            _identityServiceMock.Object,
            _notificationServiceMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenAbonentExists_ReturnsAbonentDto()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var abonent = CreateAbonentEntity(abonentId);

        _abonentRepositoryMock
            .Setup(r => r.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonent);

        // Act
        var result = await _sut.GetByIdAsync(abonentId, true);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(abonentId);

        _abonentRepositoryMock.Verify(
            r => r.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAbonentNotFound_ReturnsNull()
    {
        // Arrange
        var abonentId = Guid.NewGuid();

        _abonentRepositoryMock
            .Setup(r => r.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Abonent?)null);

        // Act
        var result = await _sut.GetByIdAsync(abonentId, true);

        // Assert
        result.ShouldBeNull();

        _abonentRepositoryMock.Verify(
            r => r.GetByIdAsync(abonentId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenAbonentsExist_ReturnsListOfAbonentDtos()
    {
        // Arrange
        var abonents = CreateAbonentEntityList(5);
        var expectedCount = abonents.Count;

        _abonentRepositoryMock
            .Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonents);

        // Act
        var result = await _sut.GetAllAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);

        _abonentRepositoryMock.Verify(
            r => r.GetAllAsync(true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoAbonentsExist_ReturnsEmptyList()
    {
        // Arrange
        var emptyList = new List<Abonent>();

        _abonentRepositoryMock
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
    public async Task CreateAsync_WhenValidDto_CreatesAbonentAndReturnsDto()
    {
        // Arrange
        var createDto = CreateCreateAbonentDto();
        var userId = Guid.NewGuid();
        var generatedPassword = "Temp123!";
        var resetToken = "reset-token-123";
        var createdUser = new User { Id = userId };

        _identityServiceMock
            .Setup(s => s.GenerateRandomPassword())
            .Returns(generatedPassword);

        _identityServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<string>(), createDto.Email, generatedPassword, "Abonent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _identityServiceMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resetToken);

        _abonentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent>());

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);

        _identityServiceMock.Verify(
            s => s.CreateUserAsync(It.IsAny<string>(), createDto.Email, generatedPassword, "Abonent", It.IsAny<CancellationToken>()),
            Times.Once);

        _abonentRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            s => s.SendPasswordSetLinkAsync(createDto.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenIdentificationNumberNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = CreateCreateAbonentDto();
        var existingAbonent = CreateAbonentEntity(Guid.NewGuid());
        existingAbonent.IdentificationNumber = createDto.IdentificationNumber;

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent> { existingAbonent });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.CreateAsync(createDto));

        exception.Message.ShouldContain(createDto.IdentificationNumber);

        _identityServiceMock.Verify(
            s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _abonentRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }


    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidDto_UpdatesAbonentAndReturnsDto()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var existingAbonent = CreateAbonentEntity(abonentId);
        var updateDto = CreateUpdateAbonentDto();

        _abonentRepositoryMock
            .Setup(r => r.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent>());

        _abonentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateAsync(abonentId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(abonentId);

        _abonentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenAbonentNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var updateDto = CreateUpdateAbonentDto();

        _abonentRepositoryMock
            .Setup(r => r.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Abonent?)null);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(abonentId, updateDto));

        _abonentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdentificationNumberChangedAndNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var existingAbonent = CreateAbonentEntity(abonentId);
        var updateDto = CreateUpdateAbonentDto();
        var otherAbonent = CreateAbonentEntity(Guid.NewGuid());
        otherAbonent.IdentificationNumber = updateDto.IdentificationNumber;

        _abonentRepositoryMock
            .Setup(r => r.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent> { otherAbonent });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.UpdateAsync(abonentId, updateDto));

        exception.Message.ShouldContain(updateDto.IdentificationNumber);

        _abonentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenAccountNumberChangedAndNotUnique_ThrowsInvalidOperationException()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        var existingAbonent = CreateAbonentEntity(abonentId);
        var updateDto = CreateUpdateAbonentDto();
        var otherAbonent = CreateAbonentEntity(Guid.NewGuid());

        _abonentRepositoryMock
            .Setup(r => r.GetByIdAsync(abonentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAbonent);

        _abonentRepositoryMock
            .SetupSequence(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent>()) // Для IdentificationNumber
            .ReturnsAsync(new List<Abonent> { otherAbonent }); // Для AccountNumber

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.UpdateAsync(abonentId, updateDto));

        _abonentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Abonent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenAbonentExists_CallsRepositoryDelete()
    {
        // Arrange
        var abonentId = Guid.NewGuid();

        _abonentRepositoryMock
            .Setup(r => r.DeleteAsync(abonentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(abonentId);

        // Assert
        _abonentRepositoryMock.Verify(
            r => r.DeleteAsync(abonentId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetWhereAsync Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredAbonents()
    {
        // Arrange
        var abonents = CreateAbonentEntityList(3);

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonents);

        // Act
        var result = await _sut.GetWhereAsync(a => a.Id == abonents.First().Id, true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(abonents.Count);

        _abonentRepositoryMock.Verify(
            r => r.GetWhereAsync(It.IsAny<Expression<Func<Abonent, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SearchByFullNameAsync Tests

    [Fact]
    public async Task SearchByFullNameAsync_WithValidSearchTerm_ReturnsMatchedAbonents()
    {
        // Arrange
        var searchTerm = "Иван";
        var abonents = CreateAbonentEntityList(3);

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(abonents);

        // Act
        var result = await _sut.SearchByFullNameAsync(searchTerm);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(abonents.Count);

        _abonentRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
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

        _abonentRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GetByContractNumberAsync Tests

    [Fact]
    public async Task GetByContractNumberAsync_WhenContractExists_ReturnsAbonentDto()
    {
        // Arrange
        var contractNumber = "CT-2024-001";
        var abonent = CreateAbonentEntity(Guid.NewGuid());
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            ContractNumber = contractNumber,
            Abonent = abonent,
            AbonentId = abonent.Id
        };

        _contractRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract> { contract });

        // Act
        var result = await _sut.GetByContractNumberAsync(contractNumber);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(abonent.Id);

        _contractRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByContractNumberAsync_WhenContractNotFound_ReturnsNull()
    {
        // Arrange
        var contractNumber = "CT-2024-999";

        _contractRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract>());

        // Act
        var result = await _sut.GetByContractNumberAsync(contractNumber);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetByContractNumberAsync_WithEmptyContractNumber_ReturnsNull(string? contractNumber)
    {
        // Act
        var result = await _sut.GetByContractNumberAsync(contractNumber!);

        // Assert
        result.ShouldBeNull();

        _contractRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Contract, bool>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region IsAbonentIdentificationNumberUniqueAsync Tests

    [Fact]
    public async Task IsAbonentIdentificationNumberUniqueAsync_WhenNumberIsUnique_ReturnsTrue()
    {
        // Arrange
        var number = "1234567890";

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent>());

        // Act
        var result = await _sut.IsAbonentIdentificationNumberUniqueAsync(number);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsAbonentIdentificationNumberUniqueAsync_WhenNumberIsNotUnique_ReturnsFalse()
    {
        // Arrange
        var number = "1234567890";
        var existingAbonent = CreateAbonentEntity(Guid.NewGuid());
        existingAbonent.IdentificationNumber = number;

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent> { existingAbonent });

        // Act
        var result = await _sut.IsAbonentIdentificationNumberUniqueAsync(number);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsAbonentIdentificationNumberUniqueAsync_WithExcludeId_ReturnsTrueWhenOnlyExcludedAbonentExists()
    {
        // Arrange
        var number = "1234567890";
        var excludeId = Guid.NewGuid();
        var existingAbonent = CreateAbonentEntity(excludeId);
        existingAbonent.IdentificationNumber = number;

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent> { existingAbonent });

        // Act
        var result = await _sut.IsAbonentIdentificationNumberUniqueAsync(number, excludeId);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task IsAbonentIdentificationNumberUniqueAsync_WithEmptyNumber_ReturnsFalse(string? number)
    {
        // Act
        var result = await _sut.IsAbonentIdentificationNumberUniqueAsync(number!);

        // Assert
        result.ShouldBeFalse();

        _abonentRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region IsAccountNumberUniqueAsync Tests

    [Fact]
    public async Task IsAccountNumberUniqueAsync_WhenNumberIsUnique_ReturnsTrue()
    {
        // Arrange
        var number = "ACC-1234567890";

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent>());

        // Act
        var result = await _sut.IsAccountNumberUniqueAsync(number);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsAccountNumberUniqueAsync_WhenNumberIsNotUnique_ReturnsFalse()
    {
        // Arrange
        var number = "ACC-1234567890";
        var existingAbonent = CreateAbonentEntity(Guid.NewGuid());
        existingAbonent.AccountNumber = number;

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent> { existingAbonent });

        // Act
        var result = await _sut.IsAccountNumberUniqueAsync(number);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsAccountNumberUniqueAsync_WithExcludeId_ReturnsTrueWhenOnlyExcludedAbonentExists()
    {
        // Arrange
        var number = "ACC-1234567890";
        var excludeId = Guid.NewGuid();
        var existingAbonent = CreateAbonentEntity(excludeId);
        existingAbonent.AccountNumber = number;

        _abonentRepositoryMock
            .Setup(r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Abonent> { existingAbonent });

        // Act
        var result = await _sut.IsAccountNumberUniqueAsync(number, excludeId);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task IsAccountNumberUniqueAsync_WithEmptyNumber_ReturnsFalse(string? number)
    {
        // Act
        var result = await _sut.IsAccountNumberUniqueAsync(number!);

        // Assert
        result.ShouldBeFalse();

        _abonentRepositoryMock.Verify(
            r => r.GetWhereAsync(
                It.IsAny<Expression<Func<Abonent, bool>>>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private static Abonent CreateAbonentEntity(Guid? id = null)
    {
        var faker = new AutoFaker<Abonent>();

        if (id.HasValue)
            faker.RuleFor(a => a.Id, _ => id.Value);

        return faker.Generate();
    }

    private static List<Abonent> CreateAbonentEntityList(int count)
    {
        var list = new List<Abonent>();
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateAbonentEntity());
        }
        return list;
    }

    private static CreateAbonentDto CreateCreateAbonentDto()
    {
        return new AutoFaker<CreateAbonentDto>().Generate();
    }

    private static UpdateAbonentDto CreateUpdateAbonentDto()
    {
        return new AutoFaker<UpdateAbonentDto>().Generate();
    }

    #endregion
}