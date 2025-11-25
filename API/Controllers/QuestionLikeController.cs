
using IceBreakerApp.Domain.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IceBreakerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuestionLikesController : ControllerBase
{
    private readonly IQuestionLikeService _service;
    private readonly ILogger<QuestionLikesController> _logger;

    public QuestionLikesController(IQuestionLikeService service, ILogger<QuestionLikesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Получить количество лайков для вопроса
    /// </summary>
    [HttpGet("question/{questionId}/count")]
    [SwaggerOperation(Summary = "Get like count for question")]
    [SwaggerResponse(200, "Success", typeof(int))]
    public async Task<ActionResult<int>> GetLikeCount(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var count = await _service.GetLikeCountAsync(questionId, cancellationToken);
        return Ok(count);
    }

    /// <summary>
    /// Получить количество лайков пользователя
    /// </summary>
    [HttpGet("user/{userId}/count")]
    [SwaggerOperation(Summary = "Get user's like count")]
    [SwaggerResponse(200, "Success", typeof(int))]
    public async Task<ActionResult<int>> GetUserLikeCount(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var count = await _service.GetUserLikeCountAsync(userId, cancellationToken);
        return Ok(count);
    }

    /// <summary>
    /// Проверить, лайкнул ли пользователь вопрос
    /// </summary>
    [HttpGet("question/{questionId}/user/{userId}")]
    [SwaggerOperation(Summary = "Check if user liked the question")]
    [SwaggerResponse(200, "Success", typeof(bool))]
    public async Task<ActionResult<bool>> HasUserLiked(
        Guid questionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var hasLiked = await _service.HasUserLikedAsync(questionId, userId, cancellationToken);
        return Ok(hasLiked);
    }

    /// <summary>
    /// Получить ID вопросов, которые лайкнул пользователь
    /// </summary>
    [HttpGet("user/{userId}/questions")]
    [SwaggerOperation(Summary = "Get user's liked question IDs")]
    [SwaggerResponse(200, "Success", typeof(List<Guid>))]
    public async Task<ActionResult<List<Guid>>> GetUserLikedQuestions(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var questionIds = await _service.GetUserLikedQuestionIdsAsync(userId, cancellationToken);
        return Ok(questionIds);
    }

    /// <summary>
    /// Поставить лайк
    /// </summary>
    [SwaggerOperation(Summary = "Like a question")]
    [SwaggerResponse(200, "Like added", typeof(bool))]
    [SwaggerResponse(409, "Like already exists")]
    [HttpPost("question/{questionId}/user/{userId}")]
    public async Task<ActionResult<bool>> AddLike(Guid questionId, Guid userId, CancellationToken ct)
    {
        var result = await _service.AddLikeAsync(questionId, userId, ct);
        return Ok(result);
    }

    [HttpDelete("question/{questionId}/user/{userId}")]
    public async Task<ActionResult<bool>> RemoveLike(Guid questionId, Guid userId, CancellationToken ct)
    {
        var result = await _service.RemoveLikeAsync(questionId, userId, ct);
        return Ok(result);
    }
}