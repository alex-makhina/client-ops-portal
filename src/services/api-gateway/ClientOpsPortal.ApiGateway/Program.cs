using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;

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
var jwksClient = new HttpClient();
Task<SecurityKey[]> keysTask = null!;

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
                if (keysTask is null)
                {
                    keysTask = jwksClient.GetStringAsync(jwksUrl)
                        .ContinueWith(t => (SecurityKey[])JsonWebKeySet.Create(t.Result).Keys.Cast<SecurityKey>().ToArray());
                }

                return keysTask.GetAwaiter().GetResult();
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
