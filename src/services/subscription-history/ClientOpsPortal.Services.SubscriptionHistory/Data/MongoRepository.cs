using ClientOpsPortal.Services.SubscriptionHistory.Configuration;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Exceptions;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Data.Interceptors;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;
using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.Data
{
    public class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;
        private readonly MongoAuditableInterceptor _auditableInterceptor;

        public MongoRepository(IMongoDatabase database, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            var collectionName = typeof(T).Name switch
            {
                nameof(SubscriptionHistoryModel) => configuration["MongoDb:SubscriptionHistoryCollection"] ?? "SubscriptionHistories",
                nameof(SubscriptionHistoryStep) => configuration["MongoDb:SubscriptionHistoryStepCollection"] ?? "SubscriptionHistorySteps",
                _ => typeof(T).Name + "s"
            };
            _collection = database.GetCollection<T>(collectionName);
            _auditableInterceptor = new MongoAuditableInterceptor(httpContextAccessor);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _collection.Find(_ => true).ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyCollection<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return await _collection.Find(predicate).ToListAsync(ct);
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            _auditableInterceptor.ApplyAuditRules(entity);
            await _collection.InsertOneAsync(entity, cancellationToken: ct);
        }

        public async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _auditableInterceptor.ApplyAuditRules(entity);
            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter, ct);

            if (result.DeletedCount == 0)
                throw new EntityNotFoundException(typeof(T), id);
        }
    }
}