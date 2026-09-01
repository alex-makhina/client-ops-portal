using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Validators;

public class UpdateSubscriptionHistoryDtoValidatorTests
{
    private readonly UpdateSubscriptionHistoryDtoValidator _validator;

    public UpdateSubscriptionHistoryDtoValidatorTests()
    {
        _validator = new UpdateSubscriptionHistoryDtoValidator();
    }

    [Fact]
    public void Validate_WhenStatusValid_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenStatusInvalid_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateSubscriptionHistoryDto
        {
            Status = (SubscriptionActionStatus)99
        };

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}