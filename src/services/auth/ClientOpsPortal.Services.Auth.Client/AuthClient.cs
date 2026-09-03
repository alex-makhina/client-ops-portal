using ClientOpsPortal.Services.Auth.Contracts;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Services.Auth.Client;

public class AuthClient : IAuthClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthClient(HttpClient http) => _http = http;

    public async Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("api/v1/users", request, ct);
        r.EnsureSuccessStatusCode();
        var response = await r.Content.ReadFromJsonAsync<CreateUserResponse>(JsonOptions, ct);
        return response!.UserId;
    }

    public async Task<UserResponse> GetUserByIdAsync(string userId, CancellationToken ct = default)
        => (await _http.GetFromJsonAsync<UserResponse>($"api/v1/users/{userId}", JsonOptions, ct))!;

    public async Task<List<UserResponse>> GetAllUsersAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<UserResponse>>("api/v1/users", JsonOptions, ct) ?? [];

    public async Task SetUserRoleAsync(SetUserRoleRequest request, CancellationToken ct = default)
    {
        var r = await _http.PutAsJsonAsync($"api/v1/users/{request.UserId}/role", request, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task BlockUserAsync(string userId, CancellationToken ct = default)
    {
        var r = await _http.PostAsync($"api/v1/users/{userId}/block", null, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task UnblockUserAsync(string userId, CancellationToken ct = default)
    {
        var r = await _http.PostAsync($"api/v1/users/{userId}/unblock", null, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string loginIdentifier, CancellationToken ct = default)
    {
        var r = await _http.PostAsJsonAsync("api/v1/auth/reset-token", new ForgotPasswordRequest { LoginIdentifier = loginIdentifier }, ct);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<ForgotPasswordResponse>(JsonOptions, ct))?.TemporaryPassword ?? string.Empty;
    }

    public async Task<string> GenerateRandomPasswordAsync(CancellationToken ct = default)
    {
        var r = await _http.GetAsync("api/v1/users/random-password", ct);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadAsStringAsync(ct);
    }
}