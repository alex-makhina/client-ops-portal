using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ClientOpsPortal.Services.Auth.Data.Seed;

public class OpenIddictClientSeeder
{
    private readonly IOpenIddictApplicationManager _manager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public OpenIddictClientSeeder(IOpenIddictApplicationManager manager, IOpenIddictScopeManager scopeManager)
    {
        _manager = manager;
        _scopeManager = scopeManager;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureScopeAsync("api", "ClientOpsPortal API", new[] { "ClientOpsPortalClient" }, ct);

        await EnsurePublicClientAsync(
            clientId: "admin-portal",
            displayName: "Admin Portal (Blazor WASM)",
            redirectUri: "http://localhost:5022/authentication/login-callback",
            postLogoutRedirectUris: new[]
            {
                "http://localhost:5110/LoggedOut?returnUrl=http://localhost:5022/"
            },
            ct);

        await EnsurePublicClientAsync(
            clientId: "personal-account",
            displayName: "Personal Account (React)",
            redirectUri: "http://localhost:62000/auth/callback",
            postLogoutRedirectUris: new[]
            {
                "http://localhost:5110/LoggedOut?returnUrl=http://localhost:62000/"
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
    }

    private async Task EnsurePublicClientAsync(string clientId, string displayName,
        string redirectUri, string[] postLogoutRedirectUris, CancellationToken ct)
    {
        var application = await _manager.FindByClientIdAsync(clientId, ct);

        if (application is null)
        {
            await _manager.CreateAsync(BuildDescriptor(clientId, displayName, redirectUri, postLogoutRedirectUris), ct);
            return;
        }

        var descriptor = BuildDescriptor(clientId, displayName, redirectUri, postLogoutRedirectUris);
        await _manager.UpdateAsync(application, descriptor, ct);
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
                Permissions.GrantTypes.Password,
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
