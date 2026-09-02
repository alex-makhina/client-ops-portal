using ClientOpsPortal.Services.Auth.Settings;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ClientOpsPortal.Services.Auth.Data.Seed;

public class OpenIddictClientSeeder
{
    private readonly IOpenIddictApplicationManager _manager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<OpenIddictClientSeeder> _logger;

    public OpenIddictClientSeeder(
        IOpenIddictApplicationManager manager,
        IOpenIddictScopeManager scopeManager,
        AuthSettings authSettings,
        ILogger<OpenIddictClientSeeder> logger)
    {
        _manager = manager;
        _scopeManager = scopeManager;
        _authSettings = authSettings;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureScopeAsync("api", "ClientOpsPortal API", new[] { "ClientOpsPortalClient" }, ct);

        await EnsurePublicClientAsync(
            clientId: "admin-portal",
            displayName: "Admin Portal (Blazor WASM)",
            redirectUri: $"{_authSettings.AdminPortalUrl}/authentication/login-callback",
            postLogoutRedirectUris: new[]
            {
                $"{_authSettings.Issuer}/LoggedOut?returnUrl={_authSettings.AdminPortalUrl}/"
            },
            ct);

        await EnsurePublicClientAsync(
            clientId: "personal-account",
            displayName: "Personal Account (React)",
            redirectUri: $"{_authSettings.PersonalAccountUrl}/auth/callback",
            postLogoutRedirectUris: new[]
            {
                $"{_authSettings.Issuer}/LoggedOut?returnUrl={_authSettings.PersonalAccountUrl}/"
            },
            ct);
    }

    private async Task EnsureScopeAsync(string name, string displayName, string[] resources, CancellationToken ct)
    {
        if (await _scopeManager.FindByNameAsync(name, ct) is not null)
            return;

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = name,
            DisplayName = displayName
        };

        foreach (var resource in resources)
            descriptor.Resources.Add(resource);

        await _scopeManager.CreateAsync(descriptor, ct);
        _logger.LogInformation("Created OpenIddict scope {ScopeName}", name);
    }

    private async Task EnsurePublicClientAsync(string clientId, string displayName,
        string redirectUri, string[] postLogoutRedirectUris, CancellationToken ct)
    {
        var application = await _manager.FindByClientIdAsync(clientId, ct);

        if (application is null)
        {
            await _manager.CreateAsync(BuildDescriptor(clientId, displayName, redirectUri, postLogoutRedirectUris), ct);
            _logger.LogInformation("Created OpenIddict client {ClientId} ({DisplayName})", clientId, displayName);
            return;
        }

        var descriptor = BuildDescriptor(clientId, displayName, redirectUri, postLogoutRedirectUris);
        await _manager.UpdateAsync(application, descriptor, ct);
        _logger.LogDebug("Updated OpenIddict client {ClientId} ({DisplayName})", clientId, displayName);
    }

    private static OpenIddictApplicationDescriptor BuildDescriptor(string clientId, string displayName,
        string redirectUri, string[] postLogoutRedirectUris)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = displayName,
            RedirectUris = { new Uri(redirectUri) },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + "api"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        foreach (var uri in postLogoutRedirectUris)
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

        return descriptor;
    }
}
