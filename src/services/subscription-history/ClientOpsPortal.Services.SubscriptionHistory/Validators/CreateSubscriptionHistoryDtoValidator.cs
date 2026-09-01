using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using FluentValidation;

namespace ClientOpsPortal.Services.SubscriptionHistory.Validators
{
    public class CreateSubscriptionHistoryDtoValidator : AbstractValidator<CreateSubscriptionHistoryDto>
    {
        public CreateSubscriptionHistoryDtoValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("SubscriptionId обязателен");

            RuleFor(x => x.AbonentId)
                .NotEmpty()
                .WithMessage("AbonentId обязателен");

            RuleFor(x => x.TariffPlanId)
                .NotEmpty()
                .WithMessage("TariffPlanId обязателен");

            RuleFor(x => x.ActionType)
                .IsInEnum()
                .WithMessage("Некорректный тип действия")
                .Must(BeValidActionType)
                .WithMessage("Тип действия должен быть Open, Close или TariffChange");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Некорректный статус");

            RuleFor(x => x.StartDate)
                .Must(BeNotInPast)
                .WithMessage("StartDate не может быть в прошлом")
                .When(x => x.StartDate != default);

            RuleFor(x => x.TariffPlanName)
                .MaximumLength(200)
                .WithMessage("TariffPlanName не должен превышать 200 символов")
                .When(x => !string.IsNullOrEmpty(x.TariffPlanName));

            RuleFor(x => x.ServiceName)
                .MaximumLength(200)
                .WithMessage("ServiceName не должен превышать 200 символов")
                .When(x => !string.IsNullOrEmpty(x.ServiceName));

            RuleFor(x => x.ContractNumber)
                .MaximumLength(50)
                .WithMessage("ContractNumber не должен превышать 50 символов")
                .When(x => !string.IsNullOrEmpty(x.ContractNumber));
        }

        private bool BeValidActionType(SubscriptionActionType actionType)
        {
            return actionType == SubscriptionActionType.Open ||
                   actionType == SubscriptionActionType.Close ||
                   actionType == SubscriptionActionType.TariffChange;
        }

        private bool BeNotInPast(DateTimeOffset startDate)
        {
            return startDate >= DateTimeOffset.UtcNow.AddMinutes(-5);
        }
    }
}