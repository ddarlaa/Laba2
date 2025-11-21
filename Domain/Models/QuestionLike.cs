// Domain/QuestionLike.cs

using IceBreakerApp.Domain.Models;

namespace IceBreakerApp.Domain;

public class QuestionLike: BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    
}