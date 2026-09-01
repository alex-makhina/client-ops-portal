using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Validators;

public class CreateSubscriptionHistoryStepDtoValidatorTests
{
    private readonly CreateSubscriptionHistoryStepDtoValidator _validator;

    public CreateSubscriptionHistoryStepDtoValidatorTests()
    {
        _validator = new CreateSubscriptionHistoryStepDtoValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = "Test message"
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenSubscriptionHistoryIdEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.Empty,
            Status = SubscriptionActionStatus.Pending
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SubscriptionHistoryId);
    }

    [Fact]
    public void Validate_WhenStatusInvalid_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = (SubscriptionActionStatus)99
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WhenMessageTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = new string('a', 1001)
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Validate_WhenMessageNull_ShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = null
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Validate_WhenMessageEmpty_ShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = string.Empty
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }
}