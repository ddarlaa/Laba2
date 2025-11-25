using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IceBreakerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(
        IQuestionService questionService,
        ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all questions", Description = "Returns paginated list of questions with filtering and sorting")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<QuestionResponseDTO>))]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<ActionResult<PaginatedResult<QuestionResponseDTO>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? search = null,
        [FromQuery] Guid? topicId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _questionService.GetAllAsync(
            pageNumber, pageSize, sortBy, sortOrder, search, topicId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get question by ID", Description = "Returns a single question by its ID")]
    [SwaggerResponse(200, "Success", typeof(QuestionResponseDTO))]
    [SwaggerResponse(404, "Question not found")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<ActionResult<QuestionResponseDTO>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var question = await _questionService.GetByIdAsync(id, cancellationToken);
        return Ok(question);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create new question", Description = "Creates a new question")]
    [SwaggerResponse(201, "Created", typeof(QuestionResponseDTO))]
    [SwaggerResponse(400, "Validation error")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<ActionResult<QuestionResponseDTO>> Create(
        [FromBody] CreateQuestionDTO dto,
        CancellationToken cancellationToken = default)
    {
        var created = await _questionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Full question update", Description = "Performs full update of a question")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Question not found")]
    [SwaggerResponse(400, "Validation error")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateQuestionDTO dto,
        CancellationToken cancellationToken = default)
    {
        await _questionService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Partial question update", Description = "Performs partial update of a question")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Question not found")]
    [SwaggerResponse(400, "Validation or patch error")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] JsonPatchDocument<UpdateQuestionDTO> patchDoc,
        CancellationToken cancellationToken = default)
    {
        if (patchDoc == null)
            return BadRequest("Patch document is required");

        await _questionService.PatchAsync(id, patchDoc, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete question", Description = "Soft deletes a question")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Question not found")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _questionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("bulk")]
    [SwaggerOperation(Summary = "Bulk create questions", Description = "Create multiple questions at once")]
    [SwaggerResponse(200, "Success with results", typeof(BulkOperationResult<QuestionResponseDTO>))]
    [SwaggerResponse(400, "Validation errors")]
    [SwaggerResponse(500, "Internal Server Error")]
    public async Task<ActionResult<BulkOperationResult<QuestionResponseDTO>>> BulkCreate(
        [FromBody] List<CreateQuestionDTO> dtos,
        CancellationToken cancellationToken = default)
    {
        var result = await _questionService.BulkCreateAsync(dtos, cancellationToken);
        return Ok(result);
    }
}
