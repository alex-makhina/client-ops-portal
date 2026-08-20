using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using MongoDB.Driver;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.Configuration
{
    public class MongoDbInitializer
    {
        public static async Task EnsureIndexesAsync(IMongoDatabase database)
        {
            var historyCollection = database.GetCollection<SubscriptionHistoryModel>("SubscriptionHistories");

            var historyIndexes = new[]
            {
                new CreateIndexModel<SubscriptionHistoryModel>(
                    Builders<SubscriptionHistoryModel>.IndexKeys.Ascending(h => h.SubscriptionId)),
                new CreateIndexModel<SubscriptionHistoryModel>(
                    Builders<SubscriptionHistoryModel>.IndexKeys.Ascending(h => h.Status)),
                new CreateIndexModel<SubscriptionHistoryModel>(
                    Builders<SubscriptionHistoryModel>.IndexKeys.Ascending(h => h.ActionType)),
                new CreateIndexModel<SubscriptionHistoryModel>(
                    Builders<SubscriptionHistoryModel>.IndexKeys.Ascending(h => h.AbonentId)),
                new CreateIndexModel<SubscriptionHistoryModel>(
                    Builders<SubscriptionHistoryModel>.IndexKeys.Combine(
                        Builders<SubscriptionHistoryModel>.IndexKeys.Ascending(h => h.SubscriptionId),
                        Builders<SubscriptionHistoryModel>.IndexKeys.Descending(h => h.CreatedAt)))
            };

            await historyCollection.Indexes.CreateManyAsync(historyIndexes);

            var stepCollection = database.GetCollection<SubscriptionHistoryStep>("SubscriptionHistorySteps");

            var stepIndexes = new[]
            {
                new CreateIndexModel<SubscriptionHistoryStep>(
                    Builders<SubscriptionHistoryStep>.IndexKeys.Ascending(s => s.SubscriptionHistoryId)),
                new CreateIndexModel<SubscriptionHistoryStep>(
                    Builders<SubscriptionHistoryStep>.IndexKeys.Ascending(s => s.Status)),
                new CreateIndexModel<SubscriptionHistoryStep>(
                    Builders<SubscriptionHistoryStep>.IndexKeys.Combine(
                        Builders<SubscriptionHistoryStep>.IndexKeys.Ascending(s => s.SubscriptionHistoryId),
                        Builders<SubscriptionHistoryStep>.IndexKeys.Descending(s => s.CreatedAt)))
            };

            await stepCollection.Indexes.CreateManyAsync(stepIndexes);
        }
    }
}
