using ClientOpsPortal.Services.SubscriptionHistory.Configuration;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using Microsoft.AspNetCore.TestHost;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

public class TestFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private CustomTestServer _server = null!;
    private IMongoDatabase _database = null!;
    private IMongoClient _mongoClient = null!;
    private bool _isInitialized = false;
    private readonly object _lock = new object();
    private readonly string _databaseName = "SubscriptionHistoryTestDb";

    public TestFixture()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .Build();
    }

    public HttpClient Client
    {
        get
        {
            lock (_lock)
            {
                EnsureServer();
                return _server.Client;
            }
        }
    }

    public IServiceProvider Services
    {
        get
        {
            lock (_lock)
            {
                EnsureServer();
                return _server.Services;
            }
        }
    }

    private void EnsureServer()
    {
        if (_server == null)
        {
            var connectionString = _mongoContainer.GetConnectionString();
            _server = new CustomTestServer(connectionString, _databaseName);
        }
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;
        }

        await _mongoContainer.StartAsync();
        _mongoClient = new MongoClient(_mongoContainer.GetConnectionString());
        _database = _mongoClient.GetDatabase(_databaseName);

        // Создаем индексы
        await MongoDbInitializer.EnsureIndexesAsync(_database);

        lock (_lock)
        {
            _isInitialized = true;
        }
    }

    public async Task DisposeAsync()
    {
        if (_server != null)
        {
            _server.Dispose();
        }
        await _mongoContainer.DisposeAsync();
    }

    public HttpClient CreateClient()
    {
        return Client;
    }

    public IMongoCollection<SubscriptionHistoryModel> GetHistoryCollection()
    {
        return _database.GetCollection<SubscriptionHistoryModel>("SubscriptionHistories");
    }

    public IMongoCollection<SubscriptionHistoryStep> GetStepCollection()
    {
        return _database.GetCollection<SubscriptionHistoryStep>("SubscriptionHistorySteps");
    }

    public async Task ClearDatabaseAsync()
    {
        try
        {
            var collections = await _database.ListCollectionNamesAsync();
            var collectionNames = await collections.ToListAsync();

            foreach (var collectionName in collectionNames)
            {
                if (collectionName != "system.indexes")
                {
                    await _database.DropCollectionAsync(collectionName);
                }
            }

            // Пересоздаем индексы после очистки
            await MongoDbInitializer.EnsureIndexesAsync(_database);
        }
        catch
        {
            // Игнорируем ошибки при очистке
        }
    }
}