using ClientOpsPortal.Web;
using ClientOpsPortal.Web.Shared.Infrastructure;
using ClientOpsPortal.Web.Shared.Providers;
using ClientOpsPortal.Web.Features.Auth.Services;
using ClientOpsPortal.Web.Features.UserManagement.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
 
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<TokenHttpMessageHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
})
.AddHttpMessageHandler<TokenHttpMessageHandler>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

await builder.Build().RunAsync();
