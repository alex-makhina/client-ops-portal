using ClientOpsPortal.Services.Directory.Contracts.Exceptions;
using ClientOpsPortal.Services.Directory.Contracts.Models;
using ClientOpsPortal.Services.Directory.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClientOpsPortal.Services.Directory.Data
{
    public class GenericRepository<T> where T : BaseEntity
    {
        protected readonly DirectoryDbContext _context;

        public GenericRepository(DirectoryDbContext context)
        {
            _context = context;
        }

        protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query) => query;

        public async Task<IReadOnlyCollection<T>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var query = _context.Set<T>().AsQueryable();
            if (withIncludes)
                query = ApplyIncludes(query);
            return await query.ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var query = _context.Set<T>().Where(x => x.Id == id);
            if (withIncludes)
                query = ApplyIncludes(query);
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyCollection<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var query = _context.Set<T>().Where(predicate);
            if (withIncludes)
                query = ApplyIncludes(query);
            return await query.ToListAsync(ct);
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.Set<T>().SingleOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null)
                throw new EntityNotFoundException(typeof(T), id);
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
