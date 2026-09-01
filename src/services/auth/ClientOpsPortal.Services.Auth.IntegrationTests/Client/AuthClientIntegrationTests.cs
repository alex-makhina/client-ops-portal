using AutoBogus;
using ClientOpsPortal.Services.Auth.Client;
using ClientOpsPortal.Services.Auth.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace ClientOpsPortal.Services.Auth.IntegrationTests.Client;

public class AuthClientIntegrationTests : IAsyncLifetime
{
    private readonly TestServer _testServer;
    private readonly HttpClient _httpClient;
    private readonly AuthClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly Dictionary<string, UserResponse> _users = new();

    public AuthClientIntegrationTests()
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddControllers();
                services.AddRouting();
                services.AddLogging();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                endpoints.MapPost("/connect/token", async context =>
                {
                    var form = await context.Request.ReadFormAsync();
                    var username = form["username"].ToString();
                    var password = form["password"].ToString();

                    if (username == "testuser" && password == "Test123!")
                    {
                        var token = CreateTestJwt(username, Guid.NewGuid().ToString(), new[] { "Manager", "Abonent" });
                        var response = new TokenResponse
                        {
                            AccessToken = token,
                            TokenType = "Bearer",
                            ExpiresIn = 3600,
                            Scope = "openid profile roles api"
                        };
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\":\"invalid_grant\"}");
                    }
                });

                    endpoints.MapPost("/api/v1/auth/reset-token", async context =>
                    {
                        string body;
                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            body = await reader.ReadToEndAsync();
                        }
                        var request = JsonSerializer.Deserialize<ForgotPasswordRequest>(body, JsonOptions);

                        if (request?.LoginIdentifier == "testuser")
                        {
                            var response = new ForgotPasswordResponse
                            {
                                TemporaryPassword = "reset-token-123456"
                            };
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("User not found");
                        }
                    });

                    endpoints.MapGet("/api/v1/users", async context =>
                    {
                        var allUsers = new List<UserResponse>();

                        allUsers.AddRange(new List<UserResponse>
                        {
                            new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserName = "testuser1",
                                Email = "test1@example.com",
                                IsBlocked = false,
                                Roles = new List<string> { "Manager" }
                            },
                            new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserName = "testuser2",
                                Email = "test2@example.com",
                                IsBlocked = true,
                                Roles = new List<string> { "Abonent" }
                            }
                        });

                        allUsers.AddRange(_users.Values);

                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync(JsonSerializer.Serialize(allUsers, JsonOptions));
                    });

                    endpoints.MapGet("/api/v1/users/{id}", async context =>
                    {
                        var id = context.Request.RouteValues["id"]?.ToString();

                        if (!string.IsNullOrEmpty(id) && _users.TryGetValue(id, out var user))
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync(JsonSerializer.Serialize(user, JsonOptions));
                        }
                        else if (id == "test-user-id")
                        {
                            var testUser = new UserResponse
                            {
                                Id = id,
                                UserName = "testuser",
                                Email = "test@example.com",
                                IsBlocked = false,
                                Roles = new List<string> { "Manager" }
                            };
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync(JsonSerializer.Serialize(testUser, JsonOptions));
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("User not found");
                        }
                    });

                    endpoints.MapPost("/api/v1/users", async context =>
                    {
                        string body;
                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            body = await reader.ReadToEndAsync();
                        }
                        var request = JsonSerializer.Deserialize<CreateUserRequest>(body, JsonOptions);

                        if (request?.UserName == "existinguser")
                        {
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync(JsonSerializer.Serialize("Username already taken", JsonOptions));
                        }
                        else
                        {
                            var userId = Guid.NewGuid().ToString();
                            var newUser = new UserResponse
                            {
                                Id = userId,
                                UserName = request?.UserName ?? "testuser",
                                Email = request?.Email ?? "test@example.com",
                                IsBlocked = false,
                                Roles = request?.Roles ?? new List<string> { "Abonent" }
                            };
                            _users[userId] = newUser;

                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonSerializer.Serialize(new CreateUserResponse { UserId = userId }, JsonOptions));
                        }
                    });

                    endpoints.MapPut("/api/v1/users/{id}/role", async context =>
                    {
                        var id = context.Request.RouteValues["id"]?.ToString();
                        string body;
                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            body = await reader.ReadToEndAsync();
                        }
                        var request = JsonSerializer.Deserialize<SetUserRoleRequest>(body, JsonOptions);

                        if (id == "test-user-id" && request?.Role == "Admin")
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("OK");
                        }
                        else if (!string.IsNullOrEmpty(id) && _users.ContainsKey(id))
                        {
                            _users[id].Roles = new List<string> { request?.Role ?? "Abonent" };
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("OK");
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("User not found");
                        }
                    });

                    endpoints.MapPost("/api/v1/users/{id}/block", async context =>
                    {
                        var id = context.Request.RouteValues["id"]?.ToString();

                        if (id == "test-user-id")
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("OK");
                        }
                        else if (!string.IsNullOrEmpty(id) && _users.ContainsKey(id))
                        {
                            _users[id].IsBlocked = true;
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("OK");
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("User not found");
                        }
                    });

                    endpoints.MapPost("/api/v1/users/{id}/unblock", async context =>
                    {
                        var id = context.Request.RouteValues["id"]?.ToString();

                        if (id == "test-user-id")
                        {
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("OK");
                        }
                        else if (!string.IsNullOrEmpty(id) && _users.ContainsKey(id))
                        {
                            _users[id].IsBlocked = false;
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("OK");
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("User not found");
                        }
                    });

                    endpoints.MapGet("/api/v1/users/random-password", async context =>
                    {
                        var password = "Test@123456";
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(password);
                    });
                });
            });

        _testServer = new TestServer(builder);
        _httpClient = _testServer.CreateClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5000/");
        _client = new AuthClient(_httpClient);
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "testuser",
            Password = "Test123!"
        };

        // Act
        var result = await _client.LoginAsync(loginRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.UserId.ShouldNotBeNullOrEmpty();
        result.UserName.ShouldBe("testuser");
        result.Roles.ShouldContain("Manager");
        result.Roles.ShouldContain("Abonent");
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidCredentials_ThrowsException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "wronguser",
            Password = "wrongpass"
        };

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "nonexistent",
            Password = "Test123!"
        };

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.LoginAsync(loginRequest));
    }

    #endregion

    #region User Management Tests

    [Fact]
    public async Task CreateUserAsync_WhenValidRequest_ReturnsUserId()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            UserName = "newuser",
            Password = "Test123!",
            Email = "newuser@example.com",
            Roles = new List<string> { "Abonent" }
        };

        // Act
        var result = await _client.CreateUserAsync(createRequest);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        Guid.TryParse(result, out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserAlreadyExists_ThrowsException()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            UserName = "existinguser",
            Password = "Test123!",
            Email = "existing@example.com",
            Roles = new List<string> { "Abonent" }
        };

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenUsersExist_ReturnsUserList()
    {
        // Act
        var result = await _client.GetAllUsersAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(2);

        var firstUser = result.First();
        firstUser.UserName.ShouldNotBeNullOrEmpty();
        firstUser.Email.ShouldNotBeNullOrEmpty();
        firstUser.Roles.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _client.GetUserByIdAsync(userId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.UserName.ShouldBe("testuser");
        result.Email.ShouldBe("test@example.com");
        result.IsBlocked.ShouldBeFalse();
        result.Roles.ShouldContain("Manager");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var userId = "nonexistent-id";

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.GetUserByIdAsync(userId));
    }

    [Fact]
    public async Task SetUserRoleAsync_WhenUserExists_DoesNotThrow()
    {
        // Arrange
        var setRoleRequest = new SetUserRoleRequest
        {
            UserId = "test-user-id",
            Role = "Admin"
        };

        // Act
        var exception = await Record.ExceptionAsync(() => _client.SetUserRoleAsync(setRoleRequest));

        // Assert
        exception.ShouldBeNull();
    }

    [Fact]
    public async Task SetUserRoleAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var setRoleRequest = new SetUserRoleRequest
        {
            UserId = "nonexistent-id",
            Role = "Admin"
        };

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.SetUserRoleAsync(setRoleRequest));
    }

    [Fact]
    public async Task BlockUserAsync_WhenUserExists_DoesNotThrow()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var exception = await Record.ExceptionAsync(() => _client.BlockUserAsync(userId));

        // Assert
        exception.ShouldBeNull();
    }

    [Fact]
    public async Task BlockUserAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var userId = "nonexistent-id";

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.BlockUserAsync(userId));
    }

    [Fact]
    public async Task UnblockUserAsync_WhenUserExists_DoesNotThrow()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var exception = await Record.ExceptionAsync(() => _client.UnblockUserAsync(userId));

        // Assert
        exception.ShouldBeNull();
    }

    [Fact]
    public async Task UnblockUserAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var userId = "nonexistent-id";

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.UnblockUserAsync(userId));
    }

    #endregion

    #region Password Tests

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_WhenUserExists_ReturnsToken()
    {
        // Arrange
        var loginIdentifier = "testuser";

        // Act
        var result = await _client.GeneratePasswordResetTokenAsync(loginIdentifier);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe("reset-token-123456");
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var loginIdentifier = "nonexistent";

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(
            () => _client.GeneratePasswordResetTokenAsync(loginIdentifier));
    }

    [Fact]
    public async Task GenerateRandomPasswordAsync_ReturnsPassword()
    {
        // Act
        var result = await _client.GenerateRandomPasswordAsync();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe("Test@123456");
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task FullUserLifecycle_WorksCorrectly()
    {
        // Arrange
        var userName = $"user_{Guid.NewGuid():N}";
        var password = "Test123!";
        var email = $"{userName}@example.com";

        var createRequest = new CreateUserRequest
        {
            UserName = userName,
            Password = password,
            Email = email,
            Roles = new List<string> { "Abonent" }
        };

        var userId = await _client.CreateUserAsync(createRequest);
        userId.ShouldNotBeNullOrEmpty();

        var user = await _client.GetUserByIdAsync(userId);
        user.ShouldNotBeNull();
        user.UserName.ShouldBe(userName);
        user.Email.ShouldBe(email);

        await _client.BlockUserAsync(userId);
        await _client.UnblockUserAsync(userId);

        var setRoleRequest = new SetUserRoleRequest
        {
            UserId = userId,
            Role = "Manager"
        };
        await _client.SetUserRoleAsync(setRoleRequest);

        var allUsers = await _client.GetAllUsersAsync();
        allUsers.ShouldContain(u => u.Id == userId);
    }

    #endregion

    public async Task InitializeAsync()
    {
        _users.Clear();
        await Task.CompletedTask;
    }

    private static string CreateTestJwt(string userName, string userId, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Name, userName),
            new(JwtRegisteredClaimNames.PreferredUsername, userName)
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        _testServer.Dispose();
        await Task.CompletedTask;
    }
}