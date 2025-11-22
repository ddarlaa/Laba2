using System.Text.Json;
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Domain;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Infrastructure.Configuration;

namespace Infrastructure.Repositories;

public class QuestionAnswerRepository : IQuestionAnswerRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public QuestionAnswerRepository(StorageSettings storageSettings)
    {
        _filePath = Path.Combine(storageSettings.StoragePath, storageSettings.QuestionAnswersFileName);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = GetNamingPolicy(storageSettings.PropertyNamingPolicy),
            WriteIndented = storageSettings.WriteIndented
        };

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    private static JsonNamingPolicy? GetNamingPolicy(string policyName) => 
        policyName.ToLower() switch
        {
            "camelcase" => JsonNamingPolicy.CamelCase,
            "snakecase" => null,
            "pascalcase" => null,
            _ => JsonNamingPolicy.CamelCase
        };

    private async Task<List<QuestionAnswer>> ReadAllAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            if (!File.Exists(_filePath))
                return new List<QuestionAnswer>();

            var json = await File.ReadAllTextAsync(_filePath, ct);
            return JsonSerializer.Deserialize<List<QuestionAnswer>>(json, _jsonOptions)
                   ?? new List<QuestionAnswer>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task WriteAllAsync(List<QuestionAnswer> items, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json, ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<QuestionAnswer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.FirstOrDefault(a => a.Id == id && a.IsActive);
    }

    public async Task<IEnumerable<QuestionAnswer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Where(a => a.IsActive);
    }

    public async Task<PaginatedResult<QuestionAnswer>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        Guid? questionId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var query = entities.Where(a => a.IsActive);

        // Фильтрация
        if (questionId.HasValue)
            query = query.Where(a => a.QuestionId == questionId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        // Сортировка (по умолчанию по дате создания)
        query = query.OrderByDescending(a => a.CreatedAt);

        // Пагинация
        var totalCount = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<QuestionAnswer>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<IEnumerable<QuestionAnswer>> GetByQuestionIdAsync(Guid questionId,
        CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities
            .Where(a => a.QuestionId == questionId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt);
    }

    public async Task<IEnumerable<QuestionAnswer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt);
    }

    public async Task<QuestionAnswer?> GetAcceptedAnswerAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.FirstOrDefault(a => a.QuestionId == questionId && a.IsAccepted && a.IsActive);
    }

    public async Task<QuestionAnswer> AddAsync(QuestionAnswer entity, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        entities.Add(entity);
        await WriteAllAsync(entities, cancellationToken);
        return entity;
    }

    public async Task<List<QuestionAnswer>> AddBulkAsync(List<QuestionAnswer> entities,
        CancellationToken cancellationToken)
    {
        var allEntities = await ReadAllAsync(cancellationToken);
        allEntities.AddRange(entities);
        await WriteAllAsync(allEntities, cancellationToken);
        return entities;
    }

    public async Task UpdateAsync(QuestionAnswer entity, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var index = entities.FindIndex(a => a.Id == entity.Id);
        if (index >= 0)
        {
            entities[index] = entity;
            await WriteAllAsync(entities, cancellationToken);
        }
    }

    public async Task MarkAsAcceptedAsync(Guid answerId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var answer = entities.FirstOrDefault(a => a.Id == answerId && a.IsActive);

        if (answer != null)
        {
            // Снимаем отметку со всех ответов на этот вопрос
            var answersForQuestion = entities.Where(a => a.QuestionId == answer.QuestionId);
            foreach (var a in answersForQuestion)
            {
                a.IsAccepted = false;
            }

            await WriteAllAsync(entities, cancellationToken);
        }
    }
}