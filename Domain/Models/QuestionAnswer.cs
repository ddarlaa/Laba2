// Domain/QuestionAnswer.cs
using System.ComponentModel.DataAnnotations;
using IceBreakerApp.Domain.Models;

namespace IceBreakerApp.Domain;

public class QuestionAnswer: BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public int ViewCount { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    
    // Доменный метод для инкремента счетчика просмотров
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // Доменный метод для мягкого удаления
    public void Delete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}