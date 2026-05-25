using ClientOpsPortal.Web.Features.UserManagement.Models;
using System.Net.Http.Json;

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
            var response = await _httpClient.GetAsync("api/v1/employees/admin/list");
            if (!response.IsSuccessStatusCode)
                return new List<UserListItem>();

            return await response.Content.ReadFromJsonAsync<List<UserListItem>>() ?? new List<UserListItem>();
        }

        public async Task<bool> CreateUserAsync(CreateUserRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/employees/admin/create", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(Guid employeeId, UpdateUserRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/employees/admin/{employeeId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleUserStatusAsync(Guid employeeId)
        {
            var response = await _httpClient.PatchAsync($"api/v1/employees/admin/{employeeId}/status", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(Guid employeeId)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/employees/admin/{employeeId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<string>> GetAvailableRolesAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/employees/admin/roles");
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        }
    }
}
