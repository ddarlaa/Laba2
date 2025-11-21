// Domain/Question.cs
using System.ComponentModel.DataAnnotations;
using IceBreakerApp.Domain.Models;

namespace IceBreakerApp.Domain;
public class Question : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid TopicId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int ViewCount { get; private set; }
    public int LikeCount { get; private set; }
    public int AnswerCount { get; private set; }
    public static Question Create(Guid userId, Guid topicId, string title, string content)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (topicId == Guid.Empty)
            throw new ArgumentException("TopicId cannot be empty", nameof(topicId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        return new Question
        {
            UserId = userId,
            TopicId = topicId,
            Title = title.Trim(),
            Content = content.Trim()
        };
    }

    public void Update(string? title, string? content, Guid? topicId)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();
        
        if (!string.IsNullOrWhiteSpace(content))
            Content = content.Trim();
        
        if (topicId.HasValue && topicId.Value != Guid.Empty)
            TopicId = topicId.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementLikeCount()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
            LikeCount--;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementAnswerCount()
    {
        AnswerCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementAnswerCount()
    {
        if (AnswerCount > 0)
            AnswerCount--;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
