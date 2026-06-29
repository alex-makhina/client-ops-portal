using ClientOpsPortal.Web;
using ClientOpsPortal.Web.Features.AbonentManagement.Services;
using ClientOpsPortal.Web.Features.Auth.Services;
using ClientOpsPortal.Web.Features.ClientCard.Services;
using ClientOpsPortal.Web.Features.ServiceManagement.Services;
using ClientOpsPortal.Web.Features.UserManagement.Services;
using ClientOpsPortal.Web.Shared.Infrastructure;
using ClientOpsPortal.Web.Shared.Providers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using ClientOpsPortal.Web.Features.Shared.Notification;

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
builder.Services.AddScoped<IAbonentManagementService, AbonentManagementService>();
builder.Services.AddScoped<IClientCardService, ClientCardService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
builder.Services.AddScoped<AppNotificationService>();

await builder.Build().RunAsync();
