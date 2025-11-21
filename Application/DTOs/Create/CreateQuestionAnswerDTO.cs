namespace IceBreakerApp.Application.DTOs.Response;

public class CreateQuestionAnswerDTO
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
}