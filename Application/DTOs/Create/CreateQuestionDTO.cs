namespace IceBreakerApp.Application.DTOs;

public class CreateQuestionDTO
{
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}