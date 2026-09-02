using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using FluentValidation;

namespace ClientOpsPortal.Services.SubscriptionHistory.Validators
{
    public class UpdateSubscriptionHistoryDtoValidator : AbstractValidator<UpdateSubscriptionHistoryDto>
    {
        public UpdateSubscriptionHistoryDtoValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Некорректный статус");
        }
    }
}