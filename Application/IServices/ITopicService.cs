using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;

namespace IceBreakerApp.Application.IServices;

public interface ITopicService
{
    Task<TopicResponseDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TopicResponseDTO>> GetAllAsync(
        int pageNumber, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<TopicResponseDTO> CreateAsync(CreateTopicDTO dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateTopicDTO dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}