using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ClientOpsPortal.Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IGenericRepository<User> _userRepository;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IGenericRepository<User> userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepository;
        }

        public async Task<User> CreateUserAsync(string userName, string email, string password, string role, CancellationToken ct = default)
        {
            var appUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(appUser, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = role, Description = $"{role} role" });
            }

            await _userManager.AddToRoleAsync(appUser, role);

            var user = new User
            {
                Id = Guid.NewGuid(),
                ExternalId = appUser.Id.ToString(),
                IdentityProvider = "Identity",
                Email = email
            };

            await _userRepository.AddAsync(user, ct);

            return user;
        }

        public async Task<string> ResetPasswordAsync(string userName, CancellationToken ct = default)
        {
            var appUser = await _userManager.FindByNameAsync(userName);
            if (appUser == null)
                throw new InvalidOperationException($"User '{userName}' not found");

            var newPassword = GenerateRandomPassword();
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            var result = await _userManager.ResetPasswordAsync(appUser, resetToken, newPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return newPassword;
        }

        public async Task BlockUserAsync(Guid applicationUserId, CancellationToken ct = default)
        {
            var appUser = await _userManager.FindByIdAsync(applicationUserId.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"ApplicationUser with ID '{applicationUserId}' not found");

            appUser.LockoutEnabled = true;
            appUser.LockoutEnd = DateTimeOffset.MaxValue;
            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to block user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task UnblockUserAsync(Guid applicationUserId, CancellationToken ct = default)
        {
            var appUser = await _userManager.FindByIdAsync(applicationUserId.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"ApplicationUser with ID '{applicationUserId}' not found");

            appUser.LockoutEnabled = false;
            appUser.LockoutEnd = null;
            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to unblock user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid applicationUserId, CancellationToken ct = default)
        {
            var appUser = await _userManager.FindByIdAsync(applicationUserId.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"ApplicationUser with ID '{applicationUserId}' not found");

            return await _userManager.GetRolesAsync(appUser);
        }

        public async Task SetUserRoleAsync(Guid applicationUserId, string role, CancellationToken ct = default)
        {
            var appUser = await _userManager.FindByIdAsync(applicationUserId.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"ApplicationUser with ID '{applicationUserId}' not found");

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = role, Description = $"{role} role" });
            }

            var currentRoles = await _userManager.GetRolesAsync(appUser);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(appUser, currentRoles);
            }

            await _userManager.AddToRoleAsync(appUser, role);
        }

        public async Task<ApplicationUser?> FindApplicationUserByExternalIdAsync(string externalId, CancellationToken ct = default)
        {
            return await _userManager.FindByIdAsync(externalId);
        }

        public string GenerateRandomPassword()
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            const string all = lower + upper + digits + special;

            var random = new Random();
            var length = random.Next(12, 16);

            var passwordChars = new char[length];
            passwordChars[0] = lower[random.Next(lower.Length)];
            passwordChars[1] = upper[random.Next(upper.Length)];
            passwordChars[2] = digits[random.Next(digits.Length)];
            passwordChars[3] = special[random.Next(special.Length)];

            for (int i = 4; i < length; i++)
            {
                passwordChars[i] = all[random.Next(all.Length)];
            }

            passwordChars = passwordChars.OrderBy(c => random.Next()).ToArray();
            return new string(passwordChars);
        }
    }
}
