using AutoMapper;
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Domain;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Domain.Interfaces.IServices;

namespace IceBreakerApp.Application.Services;

public class QuestionAnswerService : IQuestionAnswerService
{
    private readonly IQuestionAnswerRepository _answerRepository;
    private readonly IMapper _mapper;
    
    public QuestionAnswerService(IQuestionAnswerRepository answerRepository, IMapper mapper)
    {
        _answerRepository = answerRepository;
        _mapper = mapper;
    }
    
    public async Task<QuestionAnswerResponseDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var answer = await _answerRepository.GetByIdAsync(id, ct);
        if (answer == null || !answer.IsActive)
            return null;

        // Инкремент счетчика просмотров
        answer.IncrementViewCount();
        await _answerRepository.UpdateAsync(answer, ct);

        return _mapper.Map<QuestionAnswerResponseDTO>(answer);
    }
    
    public async Task<PaginatedResult<QuestionAnswerResponseDTO>> GetAllAsync(
        int pageNumber,
        int pageSize,
        Guid? questionId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _answerRepository.GetPaginatedAsync(
            pageNumber, 
            pageSize, 
            questionId, 
            userId, 
            cancellationToken);

        var dtos = _mapper.Map<List<QuestionAnswerResponseDTO>>(result.Items);
        return new PaginatedResult<QuestionAnswerResponseDTO>(dtos, result.TotalCount, pageNumber, pageSize);
    }

    public async Task<QuestionAnswerResponseDTO> CreateAsync(CreateQuestionAnswerDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<QuestionAnswer>(dto);
        var created = await _answerRepository.AddAsync(entity, cancellationToken);
        return _mapper.Map<QuestionAnswerResponseDTO>(created);
    }

    public async Task<List<QuestionAnswerResponseDTO>> BulkCreateAsync(IEnumerable<CreateQuestionAnswerDTO> dtos, CancellationToken cancellationToken = default)
    {
        var answerEntities = _mapper.Map<List<QuestionAnswer>>(dtos);
        var createdEntities = await _answerRepository.AddBulkAsync(answerEntities, cancellationToken);
        return _mapper.Map<List<QuestionAnswerResponseDTO>>(createdEntities);
    }

    public async Task UpdateAsync(Guid id, UpdateQuestionAnswerDTO dto, CancellationToken cancellationToken = default)
    {
        var answer = await _answerRepository.GetByIdAsync(id, cancellationToken);
        if (answer == null)
            throw new NotFoundException($"Answer with ID {id} not found.");
        
        if (!string.IsNullOrWhiteSpace(dto.Content))
            answer.Content = dto.Content;
            
        answer.UpdatedAt = DateTime.UtcNow;
        await _answerRepository.UpdateAsync(answer, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var answer = await _answerRepository.GetByIdAsync(id, cancellationToken);
        if (answer == null)
            throw new NotFoundException($"Answer with ID {id} not found.");
            
        answer.Delete();
        await _answerRepository.UpdateAsync(answer, cancellationToken);
    }

    public async Task AcceptAsync(Guid answerId, CancellationToken cancellationToken = default)
    {
        await _answerRepository.MarkAsAcceptedAsync(answerId, cancellationToken);
    }
    
    public async Task<QuestionAnswerResponseDTO> GetAcceptedAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var answer = await _answerRepository.GetAcceptedAnswerAsync(questionId, cancellationToken);
        if (answer == null)
            throw new NotFoundException($"Accepted answer for question {questionId} not found.");
            
        return _mapper.Map<QuestionAnswerResponseDTO>(answer);
    }
}