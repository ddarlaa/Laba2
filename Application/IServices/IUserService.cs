
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Create;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;

public interface IUserService
{
    Task<UserResponseDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<UserResponseDTO>> GetAllAsync(
        int pageNumber, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<UserResponseDTO> CreateAsync(CreateUserDTO dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateUserDTO dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponseDTO?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserResponseDTO?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
}