using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Domain.Models;

namespace IceBreakerApp.Domain.Interfaces;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Topic>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<Topic?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Topic>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Topic> AddAsync(Topic item, CancellationToken cancellationToken = default);
    Task UpdateAsync(Topic item, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Topic>> GetByIdsAsync(IEnumerable<Guid> topicIds, CancellationToken ct);
}