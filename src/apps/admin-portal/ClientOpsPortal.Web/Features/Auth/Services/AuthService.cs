using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace ClientOpsPortal.Web.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly NavigationManager _navigation;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly SignOutSessionStateManager _signOutManager;

        public AuthService(NavigationManager navigation,
            AuthenticationStateProvider authStateProvider, SignOutSessionStateManager signOutManager)
        {
            _navigation = navigation;
            _authStateProvider = authStateProvider;
            _signOutManager = signOutManager;
        }

        public void Login()
        {
            var redirectUri = _navigation.ToAbsoluteUri("authentication/login").ToString();
            _navigation.NavigateTo(redirectUri, forceLoad: true);
        }

        public async Task LogoutAsync()
        {
            await _signOutManager.SetSignOutState();
            _navigation.NavigateTo("authentication/logout", forceLoad: true);
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            return state.User.Identity?.IsAuthenticated ?? false;
        }
    }
}
