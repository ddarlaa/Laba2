namespace IceBreakerApp.Domain.Interfaces.IServices;

public interface IQuestionLikeService
{
    Task<bool> AddLikeAsync(Guid questionId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveLikeAsync(Guid questionId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetLikeCountAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<int> GetUserLikeCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasUserLikedAsync(Guid questionId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetUserLikedQuestionIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}