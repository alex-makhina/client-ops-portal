using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using FluentValidation;

namespace ClientOpsPortal.Services.SubscriptionHistory.Validators
{
    public class UpdateSubscriptionHistoryStepDtoValidator : AbstractValidator<UpdateSubscriptionHistoryStepDto>
    {
        public UpdateSubscriptionHistoryStepDtoValidator()
        {
            When(x => x.Status.HasValue, () =>
            {
                RuleFor(x => x.Status.Value)
                    .IsInEnum()
                    .WithMessage("Некорректный статус");
            });

            RuleFor(x => x.Message)
                .MaximumLength(1000)
                .WithMessage("Message не должен превышать 1000 символов")
                .When(x => !string.IsNullOrEmpty(x.Message));
        }
    }
}