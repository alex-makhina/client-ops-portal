using ClientOpsPortal.Services.SubscriptionHistory.Configuration;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Data;
using ClientOpsPortal.Services.SubscriptionHistory.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer<Guid?>(new NullableSerializer<Guid>(new GuidSerializer(BsonType.String)));

BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDb:ConnectionString"]
        ?? "mongodb://localhost:27017";
    var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);
    return new MongoClient(mongoSettings);
});

builder.Services.AddScoped(sp =>
{
    var databaseName = builder.Configuration["MongoDb:DatabaseName"]
        ?? "SubscriptionHistoryDb";
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<MongoRepository<SubscriptionHistory>>();
builder.Services.AddScoped<MongoRepository<SubscriptionHistoryStep>>();

// Регистрация сервисов
builder.Services.AddScoped<SubscriptionHistoryService>();
builder.Services.AddScoped<SubscriptionHistoryStepService>();

var jwksUrl = builder.Configuration["Jwt:JwksUrl"]
    ?? "http://localhost:5110/.well-known/jwks";
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

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    await MongoDbInitializer.EnsureIndexesAsync(database);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
