using System.Text.Json;
using IceBreakerApp.Domain;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Infrastructure.Configuration;

namespace IceBreakerApp.Infrastructure.Repositories;

public class QuestionLikeRepository : IQuestionLikeRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public QuestionLikeRepository(StorageSettings storageSettings)
    {
        _filePath = Path.Combine(storageSettings.StoragePath, storageSettings.QuestionLikesFileName);
        
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
    
    private async Task<List<QuestionLike>> ReadAllAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            if (!File.Exists(_filePath)) 
                return new List<QuestionLike>();

            var json = await File.ReadAllTextAsync(_filePath, ct);
            return JsonSerializer.Deserialize<List<QuestionLike>>(json, _jsonOptions) 
                   ?? new List<QuestionLike>();
        }
        finally 
        { 
            _semaphore.Release(); 
        }
    }

    private async Task WriteAllAsync(List<QuestionLike> items, CancellationToken ct)
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

    public async Task<QuestionLike?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.FirstOrDefault(like => like.Id == id);
    }

    public async Task<IEnumerable<QuestionLike>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Where(like => like.QuestionId == questionId);
    }

    public async Task<IEnumerable<QuestionLike>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Where(like => like.UserId == userId);
    }

    public async Task<bool> ExistsAsync(Guid questionId, Guid userId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Any(like => like.QuestionId == questionId && like.UserId == userId);
    }

    public async Task<QuestionLike> AddAsync(QuestionLike like, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        
        // Генерируем ID если не установлен
        if (like.Id == Guid.Empty)
            like.Id = Guid.NewGuid();
            
        // Устанавливаем время создания
        like.CreatedAt = DateTime.UtcNow;
        
        entities.Add(like);
        await WriteAllAsync(entities, cancellationToken);
        return like;
    }

    public async Task DeleteByQuestionAndUserAsync(Guid questionId, Guid userId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var likeToRemove = entities.FirstOrDefault(like => 
            like.QuestionId == questionId && like.UserId == userId);
            
        if (likeToRemove != null)
        {
            entities.Remove(likeToRemove);
            await WriteAllAsync(entities, cancellationToken);
        }
    }

    public async Task<int> GetCountByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Count(like => like.QuestionId == questionId);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.Count(like => like.UserId == userId);
    }

    public async Task<QuestionLike?> GetByQuestionAndUserAsync(Guid questionId, Guid userId, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        return entities.FirstOrDefault(like => 
            like.QuestionId == questionId && like.UserId == userId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entities = await ReadAllAsync(cancellationToken);
        var likeToRemove = entities.FirstOrDefault(like => like.Id == id);
            
        if (likeToRemove != null)
        {
            entities.Remove(likeToRemove);
            await WriteAllAsync(entities, cancellationToken);
        }
    }
}