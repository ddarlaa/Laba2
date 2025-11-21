using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Domain.Interfaces;
using System.Text.Json;
using IceBreakerApp.Domain.Models;
using IceBreakerApp.Infrastructure.Configuration;

namespace IceBreakerApp.Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public TopicRepository(StorageSettings storageSettings)
    {
        _filePath = Path.Combine(storageSettings.StoragePath, storageSettings.TopicsFileName);
        
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = GetNamingPolicy(storageSettings.PropertyNamingPolicy),
            WriteIndented = storageSettings.WriteIndented
        };
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    private static JsonNamingPolicy GetNamingPolicy(string policyName) =>
        policyName.ToLower() switch
        {
            "camelcase" => JsonNamingPolicy.CamelCase,
            "snakecase" => null,
            "pascalcase" => null,
            _ => JsonNamingPolicy.CamelCase
        };

    public async Task<Topic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.FirstOrDefault(t => t.Id == id);
    }

    public async Task<PaginatedResult<Topic>> GetPaginatedAsync(
        int pageNumber, 
        int pageSize, 
        string? search = null, 
        CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var query = entities.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (t.Description != null && t.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var totalCount = query.Count();
        var items = query
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<Topic>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Topic?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.FirstOrDefault(t =>
            t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Any(t =>
            t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Topic>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.OrderBy(t => t.Name);
    }

    public async Task<Topic> AddAsync(Topic item, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        entities.Add(item);
        await WriteAllAsync(entities, cancellationToken);
        return item;
    }

    public async Task UpdateAsync(Topic item, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var existingIndex = entities.FindIndex(t => t.Id == item.Id);
        
        if (existingIndex == -1)
            throw new KeyNotFoundException($"Topic with ID {item.Id} not found");

        entities[existingIndex] = item;
        await WriteAllAsync(entities, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var topicIndex = entities.FindIndex(t => t.Id == id);
        
        if (topicIndex >= 0)
        {
            entities.RemoveAt(topicIndex);
            await WriteAllAsync(entities, cancellationToken);
        }
    }

    public async Task<IReadOnlyCollection<Topic>> GetByIdsAsync(IEnumerable<Guid> topicIds, CancellationToken ct)
    {
        var topics = await ReadAllAsync(ct);
        var idSet = topicIds.ToHashSet();
    
        return topics
            .Where(t => idSet.Contains(t.Id))
            .ToList()
            .AsReadOnly();
    }

    private async Task<List<Topic>> ReadAllAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_filePath))
                return new List<Topic>();

            await using var fileStream = File.OpenRead(_filePath);
            var entities = await JsonSerializer.DeserializeAsync<List<Topic>>(fileStream, _jsonOptions, cancellationToken);
            return entities ?? new List<Topic>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log error here if needed
            return new List<Topic>();
        }
        finally 
        { 
            _semaphore.Release(); 
        }
    }

    private async Task WriteAllAsync(List<Topic> entities, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await using var fileStream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(fileStream, entities, _jsonOptions, cancellationToken);
        }
        finally 
        { 
            _semaphore.Release(); 
        }
    }
}