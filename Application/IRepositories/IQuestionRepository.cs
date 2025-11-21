using IceBreakerApp.Application.DTOs;

namespace IceBreakerApp.Domain.IRepositories;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Question>> GetAllAsync(CancellationToken ct = default);
    Task<PaginatedResult<Question>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortOrder,
        string? search,
        Guid? topicId,
        CancellationToken ct = default);
    Task<Question> AddAsync(Question question, CancellationToken ct = default);
    Task UpdateAsync(Question question, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}