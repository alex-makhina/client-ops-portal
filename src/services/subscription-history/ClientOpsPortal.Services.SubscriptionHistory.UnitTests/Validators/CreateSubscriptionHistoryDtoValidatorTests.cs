using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Validators;

public class CreateSubscriptionHistoryDtoValidatorTests
{
    private readonly CreateSubscriptionHistoryDtoValidator _validator;

    public CreateSubscriptionHistoryDtoValidatorTests()
    {
        _validator = new CreateSubscriptionHistoryDtoValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            Steps = new List<SubscriptionHistoryStep>()
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenSubscriptionIdEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.Empty,
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SubscriptionId);
    }

    [Fact]
    public void Validate_WhenAbonentIdEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.Empty,
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AbonentId);
    }

    [Fact]
    public void Validate_WhenTariffPlanIdEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.Empty,
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TariffPlanId);
    }

    [Theory]
    [InlineData(SubscriptionActionType.Open)]
    [InlineData(SubscriptionActionType.Close)]
    [InlineData(SubscriptionActionType.TariffChange)]
    public void Validate_WhenActionTypeValid_ShouldNotHaveError(SubscriptionActionType actionType)
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = actionType,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.ActionType);
    }

    [Fact]
    public void Validate_WhenActionTypeInvalid_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = (SubscriptionActionType)99,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ActionType);
    }

    [Fact]
    public void Validate_WhenStartDateInPast_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Validate_WhenTariffPlanNameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = new string('a', 201)
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TariffPlanName);
    }

    [Fact]
    public void Validate_WhenServiceNameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            ServiceName = new string('a', 201)
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ServiceName);
    }

    [Fact]
    public void Validate_WhenContractNumberTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            ContractNumber = new string('a', 51)
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ContractNumber);
    }
}