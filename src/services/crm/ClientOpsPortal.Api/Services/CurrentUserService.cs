using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ClientOpsPortal.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<User> _userRepository;
        private Guid? _userId;
        private string? _externalId;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IGenericRepository<User> userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public string ExternalId =>
            _externalId ??= _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public Guid? UserId
        {
            get
            {
                if (_userId.HasValue) return _userId;
                if (string.IsNullOrEmpty(ExternalId)) return null;

                var users = _userRepository.GetWhereAsync(u => u.ExternalId == ExternalId, false)
                    .GetAwaiter().GetResult();
                var user = users.FirstOrDefault();
                _userId = user?.Id;
                return _userId;
            }
        }
    }
}
