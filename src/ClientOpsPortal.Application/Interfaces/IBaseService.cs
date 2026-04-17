using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IBaseService<T, TDto, TCreateDto, TUpdateDto>
        where T : class
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        Task<IReadOnlyCollection<TDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default);
        Task<TDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default);
        Task<TDto> CreateAsync(TCreateDto createDto, CancellationToken ct = default);
        Task<TDto> UpdateAsync(Guid id, TUpdateDto updateDto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<TDto>> GetWhereAsync(Expression<Func<T, bool>> predicate, bool withIncludes = false, CancellationToken ct = default);
    }
}