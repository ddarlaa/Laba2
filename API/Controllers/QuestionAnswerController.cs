using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Domain.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IceBreakerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuestionAnswersController : ControllerBase
{
    private readonly IQuestionAnswerService _service;
    private readonly ILogger<QuestionAnswersController> _logger;

    public QuestionAnswersController(IQuestionAnswerService service, ILogger<QuestionAnswersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Получить пагинированный список ответов
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get paginated answers")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<QuestionAnswerResponseDTO>))]
    public async Task<ActionResult<PaginatedResult<QuestionAnswerResponseDTO>>> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? questionId = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, questionId, userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получить ответ по его ID
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get answer by ID")]
    [SwaggerResponse(200, "Success", typeof(QuestionAnswerResponseDTO))]
    [SwaggerResponse(404, "Answer not found")]
    public async Task<ActionResult<QuestionAnswerResponseDTO>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var answer = await _service.GetByIdAsync(id, cancellationToken);
        if (answer == null)
            return NotFound(new { message = $"Answer with ID {id} not found" });
            
        return Ok(answer);
    }

    /// <summary>
    /// Получить принятый ответ для вопроса
    /// </summary>
    [HttpGet("question/{questionId}/accepted")]
    [SwaggerOperation(Summary = "Get accepted answer for question")]
    [SwaggerResponse(200, "Success", typeof(QuestionAnswerResponseDTO))]
    [SwaggerResponse(404, "Accepted answer not found")]
    public async Task<ActionResult<QuestionAnswerResponseDTO>> GetAccepted(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var answer = await _service.GetAcceptedAsync(questionId, cancellationToken);
            return Ok(answer);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Создать новый ответ
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create answer")]
    [SwaggerResponse(201, "Answer created", typeof(QuestionAnswerResponseDTO))]
    [SwaggerResponse(400, "Validation error")]
    public async Task<ActionResult<QuestionAnswerResponseDTO>> Create(
        [FromBody] CreateQuestionAnswerDTO dto,
        CancellationToken cancellationToken = default)
    {
        var createdAnswer = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdAnswer.Id }, createdAnswer);
    }

    /// <summary>
    /// Массовое создание ответов
    /// </summary>
    [HttpPost("bulk")]
    [SwaggerOperation(Summary = "Bulk create answers", Description = "Creates multiple answers at once")]
    [SwaggerResponse(200, "Success", typeof(List<QuestionAnswerResponseDTO>))]
    [SwaggerResponse(400, "Validation errors")]
    public async Task<ActionResult<List<QuestionAnswerResponseDTO>>> BulkCreate(
        [FromBody] List<CreateQuestionAnswerDTO> dtos,
        CancellationToken cancellationToken = default)
    {
        var createdAnswers = await _service.BulkCreateAsync(dtos, cancellationToken);
        return Ok(createdAnswers);
    }

    /// <summary>
    /// Обновить ответ
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update answer")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Answer not found")]
    [SwaggerResponse(400, "Validation error")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateQuestionAnswerDTO dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.UpdateAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить ответ
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete answer")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Answer not found")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Отметить ответ как принятый
    /// </summary>
    [HttpPost("{id}/accept")]
    [SwaggerOperation(Summary = "Mark answer as accepted")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Answer not found")]
    public async Task<IActionResult> MarkAsAccepted(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.AcceptAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}