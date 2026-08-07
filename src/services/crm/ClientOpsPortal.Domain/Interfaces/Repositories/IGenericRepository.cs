using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ClientOpsPortal.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IReadOnlyCollection<T>> GetAllAsync(bool withIncludes, CancellationToken ct = default);

        Task<T?> GetByIdAsync(Guid id, bool withIncludes, CancellationToken ct = default);

        Task<IReadOnlyCollection<T>> GetByRangeIdAsync(IEnumerable<Guid> ids, bool withIncludes, CancellationToken ct = default);

        Task<IReadOnlyCollection<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, bool withIncludes, CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct);

        Task UpdateAsync(T entity, CancellationToken ct);

        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
