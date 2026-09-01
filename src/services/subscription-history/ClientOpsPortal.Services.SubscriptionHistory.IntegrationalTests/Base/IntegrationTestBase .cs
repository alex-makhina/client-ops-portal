using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using MongoDB.Driver;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Base;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

[Collection("SubscriptionHistory Integration Tests")]
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly TestFixture _fixture;

    public IntegrationTestBase(TestFixture fixture)
    {
        _fixture = fixture;
    }

    protected TestFixture Fixture => _fixture;

    protected HttpClient CreateClient()
    {
        return _fixture.CreateClient();
    }

    protected IMongoCollection<SubscriptionHistoryModel> GetHistoryCollection()
    {
        return _fixture.GetHistoryCollection();
    }

    protected IMongoCollection<SubscriptionHistoryStep> GetStepCollection()
    {
        return _fixture.GetStepCollection();
    }

    protected async Task ClearDatabaseAsync()
    {
        await _fixture.ClearDatabaseAsync();
    }

    public async Task InitializeAsync()
    {
        await ClearDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}