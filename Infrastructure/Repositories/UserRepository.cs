using System.Text.Json;
using IceBreakerApp.Domain.IRepositories;
using IceBreakerApp.Domain.Models;
using IceBreakerApp.Infrastructure.Configuration;

public class UserRepository : IUserRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public UserRepository(StorageSettings storageSettings)
    {
        _filePath = Path.Combine(storageSettings.StoragePath, storageSettings.UsersFileName);
        
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

    private static JsonNamingPolicy? GetNamingPolicy(string policyName) =>
        policyName.ToLower() switch
        {
            "camelcase" => JsonNamingPolicy.CamelCase,
            "snakecase" => null, // Для SnakeCase нужно будет создать свой Policy
            "pascalcase" => null, // Для PascalCase используется по умолчанию
            _ => JsonNamingPolicy.CamelCase
        };

    protected async Task WriteAllAsync(List<User> items, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(items,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json, ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }


    private async Task<List<User>> ReadAllAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            await using var fileStream = File.OpenRead(_filePath);
            var entities =
                await JsonSerializer.DeserializeAsync<List<User>>(fileStream, _jsonOptions, cancellationToken);
            return entities ?? new List<User>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log error here if needed
            return new List<User>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        return users.FirstOrDefault(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        return users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
    }

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        return users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        users.Add(user);
        await WriteAllAsync(users, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        var idx = users.FindIndex(x => x.Id == user.Id);
        if (idx == -1) throw new KeyNotFoundException();
        users[idx] = user;
        await WriteAllAsync(users, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return;
        
        // Mark user as inactive (soft delete)
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        
        await WriteAllAsync(users, ct);
    }


    public async Task<IReadOnlyList<User>> GetPageAsync(int page, int pageSize, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        return users.Where(u => u.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<IReadOnlyCollection<User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        var users = await ReadAllAsync(ct);
        var idSet = userIds.ToHashSet();

        return users
            .Where(u => idSet.Contains(u.Id) && u.IsActive)
            .ToList()
            .AsReadOnly();
    }
}