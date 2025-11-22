using System.Text.Json;
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Domain;
using IceBreakerApp.Domain.IRepositories;
using IceBreakerApp.Infrastructure.Configuration;

namespace IceBreakerApp.Infrastructure.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public QuestionRepository(StorageSettings storageSettings)
    {
        _filePath = Path.Combine(storageSettings.StoragePath, storageSettings.QuestionsFileName);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = GetNamingPolicy(storageSettings.PropertyNamingPolicy),
            WriteIndented = storageSettings.WriteIndented
        };

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    private static JsonNamingPolicy? GetNamingPolicy(string policyName) =>
        policyName.ToLower() switch
        {
            "camelcase" => JsonNamingPolicy.CamelCase,
            "snakecase" => null,
            "pascalcase" => null,
            _ => JsonNamingPolicy.CamelCase
        };

    public async Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var questions = await ReadAllAsync(ct);
        return questions.FirstOrDefault(q => q.Id == id);
    }

    public async Task<IEnumerable<Question>> GetAllAsync(CancellationToken ct = default)
    {
        return await ReadAllAsync(ct);
    }

    public async Task<PaginatedResult<Question>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortOrder,
        string? search,
        Guid? topicId,
        CancellationToken ct = default)
    {
        var allQuestions = await ReadAllAsync(ct);
        var activeQuestions = allQuestions.Where(q => q.IsActive).AsQueryable();

        // Apply filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            activeQuestions = activeQuestions.Where(q =>
                q.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                q.Content.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (topicId.HasValue)
        {
            activeQuestions = activeQuestions.Where(q => q.TopicId == topicId.Value);
        }

        // Apply sorting
        activeQuestions = ApplySorting(activeQuestions, sortBy, sortOrder);

        var totalCount = activeQuestions.Count();
        var items = activeQuestions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<Question>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Question> AddAsync(Question question, CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct);
        try
        {
            var questions = await ReadAllAsync(ct);
            questions.Add(question);
            await WriteAllAsync(questions, ct);
            return question;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAsync(Question question, CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct);
        try
        {
            var questions = await ReadAllAsync(ct);
            var existingIndex = questions.FindIndex(q => q.Id == question.Id);

            if (existingIndex >= 0)
            {
                questions[existingIndex] = question;
                await WriteAllAsync(questions, ct);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct);
        try
        {
            var questions = await ReadAllAsync(ct);
            var question = questions.FirstOrDefault(q => q.Id == id);

            if (question != null)
            {
                question.Delete();
                await WriteAllAsync(questions, ct);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private IQueryable<Question> ApplySorting(IQueryable<Question> questions, string? sortBy, string? sortOrder)
    {
        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.ToLower()) switch
        {
            "title" => isDescending ? questions.OrderByDescending(q => q.Title) : questions.OrderBy(q => q.Title),
            "createdat" => isDescending ? questions.OrderByDescending(q => q.CreatedAt) : questions.OrderBy(q => q.CreatedAt),
            "likecount" => isDescending ? questions.OrderByDescending(q => q.LikeCount) : questions.OrderBy(q => q.LikeCount),
            "viewcount" => isDescending ? questions.OrderByDescending(q => q.ViewCount) : questions.OrderBy(q => q.ViewCount),
            _ => questions.OrderByDescending(q => q.CreatedAt)
        };
    }

    private async Task<List<Question>> ReadAllAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath))
            return new List<Question>();

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, ct);
            var questions = JsonSerializer.Deserialize<List<Question>>(json, _jsonOptions);
            return questions ?? new List<Question>();
        }
        catch (JsonException)
        {
            return new List<Question>();
        }
    }

    private async Task WriteAllAsync(List<Question> questions, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(questions, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }
}
