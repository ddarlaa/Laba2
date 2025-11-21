using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Domain;

namespace IceBreakerApp.Domain.Interfaces;

public interface IQuestionAnswerRepository
{
    Task<QuestionAnswer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionAnswer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResult<QuestionAnswer>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        Guid? questionId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionAnswer>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task<IEnumerable<QuestionAnswer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<QuestionAnswer?> GetAcceptedAnswerAsync(Guid questionId, CancellationToken cancellationToken);
    Task<QuestionAnswer> AddAsync(QuestionAnswer entity, CancellationToken cancellationToken);
    Task<List<QuestionAnswer>> AddBulkAsync(List<QuestionAnswer> entities, CancellationToken cancellationToken);
    Task UpdateAsync(QuestionAnswer entity, CancellationToken cancellationToken);
    Task MarkAsAcceptedAsync(Guid answerId, CancellationToken cancellationToken);
}