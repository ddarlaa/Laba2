using AutoMapper;
using IceBreakerApp.Domain;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Domain.Interfaces.IServices;

namespace IceBreakerApp.Application.Services;

public class QuestionLikeService : IQuestionLikeService
{
    private readonly IQuestionLikeRepository _likeRepository;
    private readonly IMapper _mapper;
    
    public QuestionLikeService(IQuestionLikeRepository likeRepository, IMapper mapper)
    {
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public async Task<bool> AddLikeAsync(Guid questionId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (await _likeRepository.ExistsAsync(questionId, userId, cancellationToken))
            return false;

        var like = new QuestionLike { QuestionId = questionId, UserId = userId };
        await _likeRepository.AddAsync(like, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveLikeAsync(Guid questionId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!await _likeRepository.ExistsAsync(questionId, userId, cancellationToken))
            return false;

        await _likeRepository.DeleteByQuestionAndUserAsync(questionId, userId, cancellationToken);
        return true;
    }

    public async Task<int> GetLikeCountAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await _likeRepository.GetCountByQuestionIdAsync(questionId, cancellationToken);
    }

    public async Task<int> GetUserLikeCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _likeRepository.GetCountByUserIdAsync(userId, cancellationToken);
    }

    public async Task<bool> HasUserLikedAsync(Guid questionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _likeRepository.ExistsAsync(questionId, userId, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> GetUserLikedQuestionIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var likes = await _likeRepository.GetByUserIdAsync(userId, cancellationToken);
        return likes.Select(like => like.QuestionId).ToList();
    }
}