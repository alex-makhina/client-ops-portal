using ClientOpsPortal.Web.Features.UserManagement.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Web.Features.UserManagement.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly HttpClient _httpClient;

        public UserManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<List<UserListItem>> GetAllUsersAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/employees/list");
            if (!response.IsSuccessStatusCode)
                return new List<UserListItem>();

            return await response.Content.ReadFromJsonAsync<List<UserListItem>>() ?? new List<UserListItem>();
        }

        public async Task<(bool success, string errorMessage)> CreateUserAsync(CreateUserRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/employees/create", request);

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(errorContent) && !errorContent.TrimStart().StartsWith("{") && !errorContent.TrimStart().StartsWith("["))
                return (false, errorContent);

            return (false, $"Ошибка при создании пользователя (статус: {(int)response.StatusCode})");
        }

        public async Task<(bool success, string errorMessage)> UpdateUserAsync(Guid employeeId, UpdateUserRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/employees/{employeeId}/update", request);

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(errorContent) && !errorContent.TrimStart().StartsWith("{") && !errorContent.TrimStart().StartsWith("["))
                return (false, errorContent);

            return (false, $"Ошибка при редактировании пользователя (статус: {(int)response.StatusCode})");
        }

        public async Task<bool> ToggleUserStatusAsync(Guid employeeId)
        {
            var response = await _httpClient.PatchAsync($"api/v1/employees/{employeeId}/status", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(Guid employeeId)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/employees/{employeeId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/employees/roles");
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        }
    }
}
