using ClientOpsPortal.Web.Features.Auth.Models;
using ClientOpsPortal.Web.Shared.Providers;
using System.Net.Http.Json;

namespace ClientOpsPortal.Web.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(IHttpClientFactory httpClientFactory, CustomAuthStateProvider authStateProvider)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResponse?> LoginAsync(string login, string password)
        {
            var request = new LoginRequest
            {
                Login = login,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse != null)
            {
                await _authStateProvider.NotifyUserAuthentication(authResponse.Token);
            }

            return authResponse;
        }

        public async Task<HttpResponseMessage> SetPassword(Guid userId, string token, string newPass)
        {
            var request = new
            {
                userId = userId,
                token = token,
                newPassword = newPass
            };
            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/set-password", request);

            return response;            
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.NotifyUserLogout();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            return state.User.Identity?.IsAuthenticated ?? false;
        }
    }
}
