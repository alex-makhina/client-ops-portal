using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using FluentValidation;

namespace ClientOpsPortal.Services.SubscriptionHistory.Validators
{
    public class CreateSubscriptionHistoryStepDtoValidator : AbstractValidator<CreateSubscriptionHistoryStepDto>
    {
        public CreateSubscriptionHistoryStepDtoValidator()
        {
            RuleFor(x => x.SubscriptionHistoryId)
                .NotEmpty()
                .WithMessage("SubscriptionHistoryId обязателен");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Некорректный статус");

            RuleFor(x => x.Message)
                .MaximumLength(1000)
                .WithMessage("Message не должен превышать 1000 символов")
                .When(x => !string.IsNullOrEmpty(x.Message));
        }
    }
}