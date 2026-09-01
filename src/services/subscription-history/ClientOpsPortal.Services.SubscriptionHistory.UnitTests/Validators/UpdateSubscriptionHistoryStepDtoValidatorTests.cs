using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Validators;

public class UpdateSubscriptionHistoryStepDtoValidatorTests
{
    private readonly UpdateSubscriptionHistoryStepDtoValidator _validator;

    public UpdateSubscriptionHistoryStepDtoValidatorTests()
    {
        _validator = new UpdateSubscriptionHistoryStepDtoValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed,
            Message = "Updated message"
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenStatusNull_ShouldNotHaveError()
    {
        // Arrange
        var dto = new UpdateSubscriptionHistoryStepDto
        {
            Status = null,
            Message = "Test"
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WhenMessageTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateSubscriptionHistoryStepDto
        {
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
        var dto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Pending,
            Message = null
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }
}