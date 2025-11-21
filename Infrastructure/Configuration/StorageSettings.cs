namespace IceBreakerApp.Infrastructure.Configuration;

public class StorageSettings
{
    public string StoragePath { get; set; } = string.Empty;
    
    // Пути к файлам для каждой сущности
    public string UsersFileName { get; set; } = "users.json";
    public string QuestionsFileName { get; set; } = "questions.json";
    public string TopicsFileName { get; set; } = "topics.json";
    public string QuestionAnswersFileName { get; set; } = "questionAnswers.json";
    public string QuestionLikesFileName { get; set; } = "questionLikes.json";
    
    // JSON настройки
    public bool WriteIndented { get; set; } = true;
    public string PropertyNamingPolicy { get; set; } = "CamelCase";
}