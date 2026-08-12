using ClientOpsPortal.Services.Auth.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace ClientOpsPortal.Services.Auth.Client;

public class AuthClient : IAuthClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthClient(HttpClient http) => _http = http;

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = request.Login,
            ["password"] = request.Password,
            ["client_id"] = "admin-portal",
            ["scope"] = "openid profile roles api"
        });

        var r = await _http.PostAsync("connect/token", content, ct);
        r.EnsureSuccessStatusCode();

        var tokenResponse = await r.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Token response is null");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenResponse.AccessToken);

        return new AuthResponse
        {
            Token = tokenResponse.AccessToken,
            Roles = jwt.Claims.Where(c => c.Type is ClaimTypes.Role or "role").Select(c => c.Value).ToList(),
            UserId = jwt.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or "sub")?.Value ?? string.Empty,
            UserName = jwt.Claims.FirstOrDefault(c => c.Type is ClaimTypes.Name or "name")?.Value ?? string.Empty
        };
    }

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