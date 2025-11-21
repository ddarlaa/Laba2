namespace IceBreakerApp.Application.DTOs.Update;

public class UpdateQuestionDTO
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public Guid? TopicId { get; set; }
}