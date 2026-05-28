using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class AbonentService : IAbonentService
    {
        private readonly IGenericRepository<Abonent> _abonentRepository;
        private readonly IGenericRepository<Contract> _contractRepository;
        private readonly IIdentityService _identityService;
        private readonly INotificationService _notificationService;

        public AbonentService(
            IGenericRepository<Abonent> abonentRepository,
            IGenericRepository<Contract> contractRepository,
            IIdentityService identityService,
            INotificationService notificationService)
        {
            _abonentRepository = abonentRepository;
            _contractRepository = contractRepository;
            _identityService = identityService;
            _notificationService = notificationService;
        }

        public async Task<AbonentDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var abonent = await _abonentRepository.GetByIdAsync(id, withIncludes, ct);
            return abonent?.ToAbonentDto();
        }

        public async Task<IReadOnlyCollection<AbonentDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var abonents = await _abonentRepository.GetAllAsync(withIncludes, ct);
            return abonents.Select(a => a.ToAbonentDto()).ToList();
        }

        public async Task<AbonentDto> CreateAsync(CreateAbonentDto createDto, CancellationToken ct = default)
        {
            if (!await IsAbonentIdentificationNumberUniqueAsync(createDto.IdentificationNumber, null, ct))
                throw new InvalidOperationException($"Абонент с индентификационным номером '{createDto.IdentificationNumber}' уже существует");

            var accountNumber = GenerateAccountNumber();
            var userName = accountNumber;
            var password = _identityService.GenerateRandomPassword();

            var user = await _identityService.CreateUserAsync(userName, createDto.Email, password, "Abonent", ct);

            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(userName, ct);
            var resetLink = $"http://localhost:5022/set-password?userId={user.ExternalId}&token={Uri.EscapeDataString(resetToken)}";

            var abonent = createDto.ToEntity();
            abonent.AccountNumber = accountNumber;
            abonent.UserId = user.Id;
            await _abonentRepository.AddAsync(abonent, ct);

            await _notificationService.SendPasswordSetLinkAsync(createDto.Email, userName, resetLink, ct);

            return abonent.ToAbonentDto();
        }

        private static string GenerateAccountNumber()
        {
            var randomHex = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"ACC-{randomHex}";
        }

        public async Task<AbonentDto> UpdateAsync(Guid id, UpdateAbonentDto updateDto, CancellationToken ct = default)
        {
            var abonent = await _abonentRepository.GetByIdAsync(id, false, ct);
            if (abonent == null)
                throw new EntityNotFoundException(typeof(Abonent), id);

            if (!string.IsNullOrWhiteSpace(updateDto.IdentificationNumber) && abonent.IdentificationNumber != updateDto.IdentificationNumber)
            {
                if (!await IsAbonentIdentificationNumberUniqueAsync(updateDto.IdentificationNumber, id, ct))
                    throw new InvalidOperationException($"Абонент с индентификационным номером '{updateDto.IdentificationNumber}' уже существует");
            }

            updateDto.UpdateEntity(abonent);
            await _abonentRepository.UpdateAsync(abonent, ct);
            return abonent.ToAbonentDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _abonentRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<AbonentDto>> GetWhereAsync(Expression<Func<Abonent, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var abonents = await _abonentRepository.GetWhereAsync(predicate, withIncludes, ct);
            return abonents.Select(a => a.ToAbonentDto()).ToList();
        }

        public async Task<IReadOnlyCollection<AbonentShortDataDto>> SearchByFullNameAsync(string searchTerm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AbonentShortDataDto>();

            searchTerm = searchTerm.Trim().ToLower();

            var abonents = await _abonentRepository.GetWhereAsync(a =>
                a.FirstName.ToLower().Contains(searchTerm) ||
                a.LastName.ToLower().Contains(searchTerm) ||
                (a.MiddleName != null && a.MiddleName.ToLower().Contains(searchTerm)) ||
                (a.FirstName.ToLower() + " " + a.LastName.ToLower()).Contains(searchTerm) ||
                (a.LastName.ToLower() + " " + a.FirstName.ToLower()).Contains(searchTerm),
                false, ct);

            return abonents.Select(a => a.ToAbonentShortDataDto()).ToList();
        }

        public async Task<AbonentDto?> GetByContractNumberAsync(string contractNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(contractNumber))
                return null;

            var contract = (await _contractRepository.GetWhereAsync(
                c => c.ContractNumber == contractNumber,
                true, ct)).FirstOrDefault();

            if (contract?.Abonent == null)
                return null;

            return contract.Abonent.ToAbonentDto();
        }

        public async Task<bool> IsAbonentIdentificationNumberUniqueAsync(string number, Guid? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(number))
                return false;

            var services = await _abonentRepository.GetWhereAsync(
                s => s.IdentificationNumber.ToLower() == number.ToLower(),
                false, ct);

            if (excludeId.HasValue)
                return !services.Any(s => s.Id != excludeId.Value);

            return !services.Any();
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string number, Guid? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(number))
                return false;

            var services = await _abonentRepository.GetWhereAsync(
                s => s.AccountNumber.ToLower() == number.ToLower(),
                false, ct);

            if (excludeId.HasValue)
                return !services.Any(s => s.Id != excludeId.Value);

            return !services.Any();
        }
    }
}
