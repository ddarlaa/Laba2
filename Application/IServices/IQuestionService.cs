using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using Microsoft.AspNetCore.JsonPatch;

namespace IceBreakerApp.Application.Interfaces;

public interface IQuestionService
{
    Task<QuestionResponseDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<QuestionResponseDTO>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortOrder,
        string? search,
        Guid? topicId,
        CancellationToken ct = default);
    Task<QuestionResponseDTO> CreateAsync(CreateQuestionDTO dto, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateQuestionDTO dto, CancellationToken ct = default);
    Task PatchAsync(Guid id, JsonPatchDocument<UpdateQuestionDTO> patchDoc, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<BulkOperationResult<QuestionResponseDTO>> BulkCreateAsync(
        List<CreateQuestionDTO> dtos, 
        CancellationToken ct = default);
}
