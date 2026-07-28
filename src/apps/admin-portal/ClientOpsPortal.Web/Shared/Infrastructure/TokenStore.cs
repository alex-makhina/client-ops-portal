using Microsoft.JSInterop;

namespace ClientOpsPortal.Web.Shared.Infrastructure
{
    public class TokenStore : ITokenStore
    {private readonly IJSRuntime _js;

        public TokenStore(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");
        }

        public async Task SetTokenAsync(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            }
        }
    }

    public interface ITokenStore
    {
        Task<string?> GetTokenAsync();
        Task SetTokenAsync(string? token);
    }
}
