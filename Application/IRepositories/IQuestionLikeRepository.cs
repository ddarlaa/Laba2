using IceBreakerApp.Domain;

namespace IceBreakerApp.Domain.Interfaces;

public interface IQuestionLikeRepository
{
    Task<QuestionLike?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionLike>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task<IEnumerable<QuestionLike>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid questionId, Guid userId, CancellationToken cancellationToken);
    Task<QuestionLike> AddAsync(QuestionLike like, CancellationToken cancellationToken);
    Task DeleteByQuestionAndUserAsync(Guid questionId, Guid userId, CancellationToken cancellationToken);
    Task<int> GetCountByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}