using AutoMapper;
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.Interfaces;
using IceBreakerApp.Application.IServices;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Domain.Models;
using Microsoft.Extensions.Logging;

namespace IceBreakerApp.Application.Services;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TopicService> _logger;

    public TopicService(
        ITopicRepository topicRepository, 
        IMapper mapper,
        ILogger<TopicService> logger)
    {
        _topicRepository = topicRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TopicResponseDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken);
        return topic != null ? _mapper.Map<TopicResponseDTO>(topic) : null;
    }

    public async Task<PaginatedResult<TopicResponseDTO>> GetAllAsync(
        int pageNumber, 
        int pageSize, 
        string? search = null, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));

        var result = await _topicRepository.GetPaginatedAsync(pageNumber, pageSize, search, cancellationToken);
        var dtos = _mapper.Map<List<TopicResponseDTO>>(result.Items);
        
        return new PaginatedResult<TopicResponseDTO>(dtos, result.TotalCount, pageNumber, pageSize);
    }

    public async Task<TopicResponseDTO> CreateAsync(CreateTopicDTO dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Topic name is required", nameof(dto.Name));

        var existingTopic = await _topicRepository.FindByNameAsync(dto.Name.Trim(), cancellationToken);
        if (existingTopic != null)
            throw new InvalidOperationException($"Topic with name '{dto.Name}' already exists");

        var topic = _mapper.Map<Topic>(dto);
        
        // Set domain properties
        topic.Name = topic.Name.Trim();

        var created = await _topicRepository.AddAsync(topic, cancellationToken);
        return _mapper.Map<TopicResponseDTO>(created);
    }

    public async Task UpdateAsync(Guid id, UpdateTopicDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Topic name cannot be empty", nameof(dto.Name));

        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken);
        if (topic == null)
            throw new KeyNotFoundException($"Topic with ID {id} not found");

        // Check name uniqueness if name is being updated
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != topic.Name)
        {
            var existingTopic = await _topicRepository.FindByNameAsync(dto.Name.Trim(), cancellationToken);
            if (existingTopic != null && existingTopic.Id != id)
                throw new InvalidOperationException($"Topic with name '{dto.Name}' already exists");
        }

        // Apply updates
        if (!string.IsNullOrWhiteSpace(dto.Name)) 
            topic.Name = dto.Name.Trim();
            
        if (dto.Description != null) 
            topic.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();


        await _topicRepository.UpdateAsync(topic, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken);
        if (topic == null)
            throw new KeyNotFoundException($"Topic with ID {id} not found");

        await _topicRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.FindByNameAsync(name, cancellationToken);
        return topic != null;
    }
}