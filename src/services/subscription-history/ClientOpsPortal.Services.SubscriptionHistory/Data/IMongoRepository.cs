using System.Linq.Expressions;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;

namespace ClientOpsPortal.Services.SubscriptionHistory.Data
{
    public interface IMongoRepository<T> where T : BaseEntity
    {
        Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken ct = default);
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}