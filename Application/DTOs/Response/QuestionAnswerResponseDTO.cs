namespace IceBreakerApp.Application.DTOs.Update;

public class QuestionAnswerResponseDTO
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; }
}