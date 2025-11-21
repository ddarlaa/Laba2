using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Create;
using IceBreakerApp.Application.DTOs.ListItem;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IceBreakerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    /// <summary>
    /// Получить всех пользователей с пагинацией и поиском.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get paginated users")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<UserListItemDTO>))]
    public async Task<ActionResult<PaginatedResult<UserListItemDTO>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получить пользователя по ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get user by ID")]
    [SwaggerResponse(200, "Success", typeof(UserResponseDTO))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult<UserResponseDTO>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _service.GetByIdAsync(id, cancellationToken);
        return user == null
            ? NotFound(new { message = $"User with ID {id} not found" })
            : Ok(user);
    }

    /// <summary>
    /// Создать пользователя.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create new user")]
    [SwaggerResponse(201, "Created", typeof(UserResponseDTO))]
    [SwaggerResponse(400, "Validation error or duplicate email/username")]
    public async Task<ActionResult<UserResponseDTO>> Create(
        [FromBody] CreateUserDTO dto,
        CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновить пользователя.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update user")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Validation error or duplicate email/username")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody]  UpdateUserDTO dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.UpdateAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
    }

    /// <summary>
    /// Удалить пользователя.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete user")]
    [SwaggerResponse(204, "No Content")]
    [SwaggerResponse(404, "User not found")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
    }
}
