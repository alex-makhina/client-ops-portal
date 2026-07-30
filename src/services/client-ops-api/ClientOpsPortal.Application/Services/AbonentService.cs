using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Services.Auth.Client;
using ClientOpsPortal.Services.Auth.Contracts;
using System.Linq.Expressions;
using ClientOpsPortal.Application.DTOs.Common;

namespace ClientOpsPortal.Application.Services
{
    public class AbonentService : IAbonentService
    {
        private readonly IGenericRepository<Abonent> _abonentRepository;
        private readonly IGenericRepository<Contract> _contractRepository;
        private readonly IAuthClient _authClient;
        private readonly IGenericRepository<User> _userRepository;

        public AbonentService(
            IGenericRepository<Abonent> abonentRepository,
            IGenericRepository<Contract> contractRepository,
            IAuthClient authClient,
            IGenericRepository<User> userRepository)
        {
            _abonentRepository = abonentRepository;
            _contractRepository = contractRepository;
            _authClient = authClient;
            _userRepository = userRepository;
        }

        public async Task<AbonentDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var abonent = await _abonentRepository.GetByIdAsync(id, withIncludes, ct);
            return abonent?.ToAbonentDto();
        }

        public async Task<AbonentDto?> GetByIdUserAsync(string externalId, CancellationToken ct = default)
        {
            var users = await _userRepository.GetWhereAsync(u => u.ExternalId == externalId, false, ct);
            var user = users.FirstOrDefault();
            if (user == null) return null;
            var abonents = await _abonentRepository.GetWhereAsync(x => x.UserId == user.Id, false, ct);
            return abonents.Select(a => a.ToAbonentDto()).FirstOrDefault();
        }

        public async Task<IReadOnlyCollection<AbonentDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var abonents = await _abonentRepository.GetAllAsync(withIncludes, ct);
            return abonents.Select(a => a.ToAbonentDto()).ToList();
        }

        public async Task<AbonentDto> CreateAsync(CreateAbonentDto createDto, CancellationToken ct = default)
        {
            if (!await IsAbonentIdentificationNumberUniqueAsync(createDto.IdentificationNumber, null, ct))
                throw new InvalidOperationException($"РђР±РѕРЅРµРЅС‚ СЃ РёРЅРґРµРЅС‚РёС„РёРєР°С†РёРѕРЅРЅС‹Рј РЅРѕРјРµСЂРѕРј '{createDto.IdentificationNumber}' СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");

            var accountNumber = GenerateAccountNumber();
            var userName = accountNumber;
            var password = await _authClient.GenerateRandomPasswordAsync(ct);
            var userId = await _authClient.CreateUserAsync(new CreateUserRequest
            {
                UserName = userName,
                Password = password,
                Email = createDto.Email,
                Roles = new List<string> { "Abonent" }
            }, ct);

            var user = new User { Id = Guid.NewGuid(), ExternalId = userId };
            await _userRepository.AddAsync(user, ct);

            var abonent = createDto.ToEntity();
            abonent.AccountNumber = accountNumber;
            abonent.UserId = user.Id;
            await _abonentRepository.AddAsync(abonent, ct);
            return abonent.ToAbonentDto();
        }

        public async Task<AbonentDto> UpdateAsync(Guid id, UpdateAbonentDto updateDto, CancellationToken ct = default)
        {
            var abonent = await _abonentRepository.GetByIdAsync(id, false, ct);
            if (abonent == null) throw new EntityNotFoundException(typeof(Abonent), id);
            updateDto.UpdateEntity(abonent);
            await _abonentRepository.UpdateAsync(abonent, ct);
            return abonent.ToAbonentDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default) => await _abonentRepository.DeleteAsync(id, ct);

        public async Task<IReadOnlyCollection<AbonentDto>> GetWhereAsync(Expression<Func<Abonent, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var abonents = await _abonentRepository.GetWhereAsync(predicate, withIncludes, ct);
            return abonents.Select(a => a.ToAbonentDto()).ToList();
        }

        

        private static string GenerateAccountNumber()
        {
            var year = DateTime.UtcNow.Year;
            var ticks = DateTime.UtcNow.Ticks.ToString("D19")[^8..];
            var random = new Random().Next(100, 999);
            return $"AB-{year}-{ticks}-{random}";
        }

        public async Task<bool> IsAbonentIdentificationNumberUniqueAsync(string identificationNumber, Guid? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identificationNumber)) return false;
            var existing = await _abonentRepository.GetWhereAsync(a => a.IdentificationNumber == identificationNumber, false, ct);
            if (excludeId.HasValue) return !existing.Any(a => a.Id != excludeId.Value);
            return !existing.Any();
        }

        public async Task<IReadOnlyCollection<AbonentShortDataDto>> SearchByFullNameAsync(string searchTerm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<AbonentShortDataDto>();
            searchTerm = searchTerm.Trim().ToLower();
            var abonents = await _abonentRepository.GetWhereAsync(a =>
                a.LastName.ToLower().Contains(searchTerm) || a.FirstName.ToLower().Contains(searchTerm) || (a.MiddleName != null && a.MiddleName.ToLower().Contains(searchTerm)), false, ct);
            return abonents.Select(a => a.ToAbonentShortDataDto()).ToList();
        }

        public async Task<AbonentDto?> GetByContractNumberAsync(string contractNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(contractNumber)) return null;
            var contracts = await _contractRepository.GetWhereAsync(c => c.ContractNumber == contractNumber, true, ct);
            var contract = contracts.FirstOrDefault();
            return contract?.Abonent?.ToAbonentDto();
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, Guid? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) return false;
            var existing = await _abonentRepository.GetWhereAsync(a => a.AccountNumber == accountNumber, false, ct);
            if (excludeId.HasValue) return !existing.Any(a => a.Id != excludeId.Value);
            return !existing.Any();
        }
    }
}