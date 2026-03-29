using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class GenericRepository<T>(ClientOpsPortalDbContext context) : IGenericRepository<T> where T : BaseEntity
    {
        protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query) => query;

        public async Task AddAsync(T entity, CancellationToken ct)
        {
            await context.Set<T>().AddAsync(entity, ct);
            await context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var entity = await context.Set<T>().SingleOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null)
                throw new EntityNotFoundException(typeof(T), id);

            context.Set<T>().Remove(entity);

            await context.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(bool withIncludes, CancellationToken ct = default)
        {
            var query = context.Set<T>().AsQueryable();

            if (withIncludes)
                query = ApplyIncludes(query);

            return await query.ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, bool withIncludes, CancellationToken ct = default)
        {
            var query = context.Set<T>().Where(x => x.Id == id);

            if (withIncludes)
                query = ApplyIncludes(query);

            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyCollection<T>> GetByRangeIdAsync(IEnumerable<Guid> ids, bool withIncludes, CancellationToken ct = default)
        {
            if (!ids.Any())
                return [];

            var query = context.Set<T>().Where(x => ids.Contains(x.Id));

            if (withIncludes)
                query = ApplyIncludes(query);

            return await query.ToListAsync(ct);
        }

        public async Task<IReadOnlyCollection<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, bool withIncludes, CancellationToken ct = default)
        {
            var query = context.Set<T>().Where(predicate);

            if (withIncludes)
                query = ApplyIncludes(query);

            return await query.ToListAsync(ct);
        }

        public async Task UpdateAsync(T entity, CancellationToken ct)
        {
            context.Set<T>().Update(entity);

            await context.SaveChangesAsync(ct);
        }
    }
}
