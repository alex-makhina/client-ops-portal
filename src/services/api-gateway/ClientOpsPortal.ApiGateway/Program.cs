using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5022", "http://localhost:62000")
              .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var jwksUrl = builder.Configuration["Jwt:JwksUrl"] ?? "http://localhost:5110/.well-known/jwks";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:5110";
var audience = builder.Configuration["Jwt:Audience"] ?? "ClientOpsPortalClient";

var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
    jwksUrl,
    new JwksConfigurationRetriever(),
    new HttpDocumentRetriever { RequireHttps = false });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var config = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
                return config.SigningKeys;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();

sealed class JwksConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
    {
        var json = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
        var keySet = new JsonWebKeySet(json);
        var configuration = new OpenIdConnectConfiguration { JsonWebKeySet = keySet };
        foreach (var key in keySet.GetSigningKeys())
            configuration.SigningKeys.Add(key);
        return configuration;
    }
}
