using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;

namespace IceBreakerApp.Domain.Interfaces.IServices;

public interface IQuestionAnswerService
{
    public Task<QuestionAnswerResponseDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
    public Task<PaginatedResult<QuestionAnswerResponseDTO>> GetAllAsync(
        int pageNumber,
        int pageSize,
        Guid? questionId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    public Task<QuestionAnswerResponseDTO> CreateAsync(CreateQuestionAnswerDTO dto,
        CancellationToken cancellationToken = default);

    public Task<List<QuestionAnswerResponseDTO>> BulkCreateAsync(IEnumerable<CreateQuestionAnswerDTO> dtos,
        CancellationToken cancellationToken = default);

    public Task UpdateAsync(Guid id, UpdateQuestionAnswerDTO dto, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    public Task AcceptAsync(Guid answerId, CancellationToken cancellationToken = default);
    public Task<QuestionAnswerResponseDTO> GetAcceptedAsync(Guid questionId,
        CancellationToken cancellationToken = default);
}
