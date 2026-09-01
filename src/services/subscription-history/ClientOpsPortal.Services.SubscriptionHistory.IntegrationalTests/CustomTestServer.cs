using ClientOpsPortal.Services.Subscription.Controllers;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Data;
using ClientOpsPortal.Services.SubscriptionHistory.Data.Interceptors;
using ClientOpsPortal.Services.SubscriptionHistory.Services;
using ClientOpsPortal.Services.SubscriptionHistory.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

public class CustomTestServer : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;
    private bool _disposed;

    public CustomTestServer(string connectionString, string databaseName = "SubscriptionHistoryTestDb")
    {
        RegisterSerializers();

        var pack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new CamelCaseElementNameConvention()
        };
        ConventionRegistry.Register("CustomConventions", pack, t => true);

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        var descriptors = services
                            .Where(d => d.ServiceType == typeof(IMongoClient) ||
                                       d.ServiceType == typeof(IMongoDatabase))
                            .ToList();

                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddSingleton<IMongoClient>(sp =>
                        {
                            var settings = MongoClientSettings.FromConnectionString(connectionString);
                            return new MongoClient(settings);
                        });

                        services.AddScoped(sp =>
                        {
                            var client = sp.GetRequiredService<IMongoClient>();
                            return client.GetDatabase(databaseName);
                        });

                        services.AddHttpContextAccessor();

                        services.AddScoped<MongoAuditableInterceptor>();

                        services.AddScoped<IMongoRepository<SubscriptionHistoryModel>, MongoRepository<SubscriptionHistoryModel>>();
                        services.AddScoped<IMongoRepository<SubscriptionHistoryStep>, MongoRepository<SubscriptionHistoryStep>>();

                        services.AddScoped<SubscriptionHistoryService>();
                        services.AddScoped<SubscriptionHistoryStepService>();

                        services.AddScoped<IValidator<CreateSubscriptionHistoryDto>, CreateSubscriptionHistoryDtoValidator>();
                        services.AddScoped<IValidator<UpdateSubscriptionHistoryDto>, UpdateSubscriptionHistoryDtoValidator>();
                        services.AddScoped<IValidator<CreateSubscriptionHistoryStepDto>, CreateSubscriptionHistoryStepDtoValidator>();
                        services.AddScoped<IValidator<UpdateSubscriptionHistoryStepDto>, UpdateSubscriptionHistoryStepDtoValidator>();

                        services.AddLogging();

                        services.AddControllers()
                            .AddApplicationPart(typeof(SubscriptionHistoriesController).Assembly)
                            .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                            });

                        services.AddProblemDetails();
                        services.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
            });

        var host = hostBuilder.Build();
        host.Start();

        _server = host.GetTestServer();
        _client = _server.CreateClient();
        _services = _server.Services;
    }

    private static void RegisterSerializers()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        }
        catch { }

        try
        {
            BsonSerializer.RegisterSerializer<Guid?>(new NullableSerializer<Guid>(new GuidSerializer(BsonType.String)));
        }
        catch { }

        try
        {
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
        }
        catch { }

        try
        {
            BsonSerializer.RegisterSerializer<DateTimeOffset?>(new NullableSerializer<DateTimeOffset>(new DateTimeOffsetSerializer(BsonType.String)));
        }
        catch { }
    }

    public HttpClient Client => _client;
    public IServiceProvider Services => _services;

    public void Dispose()
    {
        if (!_disposed)
        {
            _client?.Dispose();
            _server?.Dispose();
            _disposed = true;
        }
    }
}