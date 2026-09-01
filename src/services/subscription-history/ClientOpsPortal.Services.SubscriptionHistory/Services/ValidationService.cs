using FluentValidation;
using FluentValidation.Results;

namespace ClientOpsPortal.Services.SubscriptionHistory.Services
{
    public class ValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ValidationResult> ValidateAsync<T>(T instance, CancellationToken ct = default)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();
            if (validator == null)
                return new ValidationResult();

            return await validator.ValidateAsync(instance, ct);
        }

        public async Task ThrowIfInvalidAsync<T>(T instance, CancellationToken ct = default)
        {
            var result = await ValidateAsync(instance, ct);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }
    }
}