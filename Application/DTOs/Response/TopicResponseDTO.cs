namespace IceBreakerApp.Application.DTOs.Update;

public class TopicResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}