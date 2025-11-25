using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.ListItem;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace IceBreakerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _service;
    private readonly ILogger<TopicsController> _logger;

    public TopicsController(ITopicService service, ILogger<TopicsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Получить все темы с пагинацией и поиском
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get paginated topics")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<TopicListItemDTO>))]
    public async Task<ActionResult<PaginatedResult<TopicListItemDTO>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получить тему по ID
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get topic by ID")]
    [SwaggerResponse(200, "Success", typeof(TopicResponseDTO))]
    [SwaggerResponse(404, "Topic not found")]
    public async Task<ActionResult<TopicResponseDTO>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var topic = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(topic);
    }

    /// <summary>
    /// Создать тему
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create new topic")]
    [SwaggerResponse(201, "Created", typeof(TopicResponseDTO))]
    [SwaggerResponse(400, "Validation error or duplicate name")]
    public async Task<ActionResult<TopicResponseDTO>> Create(
        [FromBody] CreateTopicDTO dto,
        CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновить тему
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update topic")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Topic not found")]
    [SwaggerResponse(400, "Validation error or duplicate name")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTopicDTO dto,
        CancellationToken cancellationToken = default)
    {
        await _service.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удалить тему
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete topic")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "Topic not found")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}