using FluentAssertions;
using Moq;
using AutoFixture;
using IceBreakerApp.Domain;
using IceBreakerApp.Infrastructure.Configuration;
using IceBreakerApp.Infrastructure.Repositories;
using Infrastructure.Repositories;
using IceBreakerApp.Domain.Models;

namespace IceBreakerApp.Tests;

// =============================================
// ТЕСТЫ ДЛЯ РЕПОЗИТОРИЕВ
// =============================================

public class QuestionRepositoryTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly string _testFilePath;
    private readonly QuestionRepository _repository;

    public QuestionRepositoryTests()
    {
        _fixture = new Fixture();
            
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_questions_{Guid.NewGuid()}.json");
        
        var storageSettingsMock = new Mock<StorageSettings>();
        storageSettingsMock.Setup(x => x.StoragePath).Returns(Path.GetTempPath());
        storageSettingsMock.Setup(x => x.QuestionsFileName).Returns(Path.GetFileName(_testFilePath));
        storageSettingsMock.Setup(x => x.PropertyNamingPolicy).Returns("camelcase");
        storageSettingsMock.Setup(x => x.WriteIndented).Returns(true);

        _repository = new QuestionRepository(storageSettingsMock.Object);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public async Task GetByIdAsync_WhenQuestionExists_ReturnsQuestion()
    {
        // Arrange
        var expectedQuestion = _fixture.Create<Question>();
        var questions = new List<Question> { expectedQuestion };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(questions));

        // Act
        var result = await _repository.GetByIdAsync(expectedQuestion.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedQuestion.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenQuestionDoesNotExist_ReturnsNull()
    {
        // Arrange
        var questions = _fixture.CreateMany<Question>(3).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(questions));

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsQuestionToFile()
    {
        // Arrange
        var newQuestion = _fixture.Create<Question>();
        await File.WriteAllTextAsync(_testFilePath, "[]");

        // Act
        var result = await _repository.AddAsync(newQuestion);

        // Assert
        result.Should().BeEquivalentTo(newQuestion);
        
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var questionsInFile = System.Text.Json.JsonSerializer.Deserialize<List<Question>>(fileContent);
        questionsInFile.Should().ContainSingle();
        questionsInFile![0].Should().BeEquivalentTo(newQuestion);
    }

    [Fact]
    public async Task UpdateAsync_WhenQuestionExists_UpdatesQuestion()
    {
        // Arrange
        var originalQuestion = _fixture.Create<Question>();
        var updatedQuestion = _fixture.Build<Question>()
            .With(x => x.Id, originalQuestion.Id)
            .With(x => x.Title, "Updated Title")
            .Create();
            
        var questions = new List<Question> { originalQuestion };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(questions));

        // Act
        await _repository.UpdateAsync(updatedQuestion);

        // Assert
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var questionsInFile = System.Text.Json.JsonSerializer.Deserialize<List<Question>>(fileContent);
        questionsInFile.Should().ContainSingle();
        questionsInFile![0].Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_WhenQuestionExists_MarksAsInactive()
    {
        // Arrange
        var question = _fixture.Build<Question>()
            .With(x => x.IsActive, true)
            .Create();
            
        var questions = new List<Question> { question };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(questions));

        // Act
        await _repository.DeleteAsync(question.Id);

        // Assert
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var questionsInFile = System.Text.Json.JsonSerializer.Deserialize<List<Question>>(fileContent);
        questionsInFile.Should().ContainSingle();
        questionsInFile![0].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllQuestions()
    {
        // Arrange
        var questions = _fixture.CreateMany<Question>(5).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(questions));

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task ReadAllAsync_WhenFileIsCorrupted_ReturnsEmptyList()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "invalid json content");

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var enumerable = result as Question[] ?? result.ToArray();
        enumerable.Should().NotBeNull();
        enumerable.Should().BeEmpty();
    }
}

public class QuestionAnswerRepositoryTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly string _testFilePath;
    private readonly QuestionAnswerRepository _repository;

    public QuestionAnswerRepositoryTests()
    {
        _fixture = new Fixture();
            
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_answers_{Guid.NewGuid()}.json");
        
        var storageSettingsMock = new Mock<StorageSettings>();
        storageSettingsMock.Setup(x => x.StoragePath).Returns(Path.GetTempPath());
        storageSettingsMock.Setup(x => x.QuestionAnswersFileName).Returns(Path.GetFileName(_testFilePath));
        storageSettingsMock.Setup(x => x.PropertyNamingPolicy).Returns("camelcase");
        storageSettingsMock.Setup(x => x.WriteIndented).Returns(true);

        _repository = new QuestionAnswerRepository(storageSettingsMock.Object);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public async Task GetByQuestionIdAsync_ReturnsOnlyActiveAnswersForQuestion()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var activeAnswers = _fixture.Build<QuestionAnswer>()
            .With(x => x.QuestionId, questionId)
            .With(x => x.IsActive, true)
            .CreateMany(3)
            .ToList();
            
        var inactiveAnswers = _fixture.Build<QuestionAnswer>()
            .With(x => x.QuestionId, questionId)
            .With(x => x.IsActive, false)
            .CreateMany(2)
            .ToList();
            
        var otherQuestionAnswers = _fixture.Build<QuestionAnswer>()
            .With(x => x.IsActive, true)
            .CreateMany(2)
            .ToList();
            
        var allAnswers = activeAnswers.Concat(inactiveAnswers).Concat(otherQuestionAnswers).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(allAnswers));

        // Act
        var result = await _repository.GetByQuestionIdAsync(questionId, CancellationToken.None);

        // Assert
        var questionAnswers = result as QuestionAnswer[] ?? result.ToArray();
        questionAnswers.Should().HaveCount(3);
        questionAnswers.All(a => a.QuestionId == questionId && a.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetAcceptedAnswerAsync_ReturnsAcceptedAnswer()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var acceptedAnswer = _fixture.Build<QuestionAnswer>()
            .With(x => x.QuestionId, questionId)
            .With(x => x.IsAccepted, true)
            .With(x => x.IsActive, true)
            .Create();
            
        var otherAnswers = _fixture.Build<QuestionAnswer>()
            .With(x => x.QuestionId, questionId)
            .With(x => x.IsAccepted, false)
            .With(x => x.IsActive, true)
            .CreateMany(3)
            .ToList();
            
        var allAnswers = otherAnswers.Append(acceptedAnswer).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(allAnswers));

        // Act
        var result = await _repository.GetAcceptedAnswerAsync(questionId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(acceptedAnswer.Id);
        result.IsAccepted.Should().BeTrue();
    }

    [Fact]
    public async Task AddBulkAsync_AddsMultipleAnswers()
    {
        // Arrange
        var existingAnswers = _fixture.CreateMany<QuestionAnswer>(2).ToList();
        var newAnswers = _fixture.CreateMany<QuestionAnswer>(3).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(existingAnswers));

        // Act
        var result = await _repository.AddBulkAsync(newAnswers, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var allAnswers = System.Text.Json.JsonSerializer.Deserialize<List<QuestionAnswer>>(fileContent);
        allAnswers.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAnswerExists_ReturnsAnswer()
    {
        // Arrange
        var expectedAnswer = _fixture.Build<QuestionAnswer>()
            .With(x => x.IsActive, true)
            .Create();
        var answers = new List<QuestionAnswer> { expectedAnswer };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(answers));

        // Act
        var result = await _repository.GetByIdAsync(expectedAnswer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedAnswer.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAnswerInactive_ReturnsNull()
    {
        // Arrange
        var inactiveAnswer = _fixture.Build<QuestionAnswer>()
            .With(x => x.IsActive, false)
            .Create();
        var answers = new List<QuestionAnswer> { inactiveAnswer };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(answers));

        // Act
        var result = await _repository.GetByIdAsync(inactiveAnswer.Id);

        // Assert
        result.Should().BeNull();
    }
}

public class UserRepositoryTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly Mock<StorageSettings> _storageSettingsMock;
    private readonly string _testFilePath;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _fixture = new Fixture();
            
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_users_{Guid.NewGuid()}.json");
        
        _storageSettingsMock = new Mock<StorageSettings>();
        _storageSettingsMock.Setup(x => x.StoragePath).Returns(Path.GetTempPath());
        _storageSettingsMock.Setup(x => x.UsersFileName).Returns(Path.GetFileName(_testFilePath));
        _storageSettingsMock.Setup(x => x.PropertyNamingPolicy).Returns("camelcase");
        _storageSettingsMock.Setup(x => x.WriteIndented).Returns(true);

        _repository = new UserRepository(_storageSettingsMock.Object);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public async Task FindByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = _fixture.Build<User>()
            .With(x => x.Email, email)
            .With(x => x.IsActive, true)
            .Create();
            
        var users = new List<User> { user };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(users));

        // Act
        var result = await _repository.FindByEmailAsync(email, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task FindByUsernameAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var username = "testuser";
        var user = _fixture.Build<User>()
            .With(x => x.Username, username)
            .With(x => x.IsActive, true)
            .Create();
            
        var users = new List<User> { user };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(users));

        // Act
        var result = await _repository.FindByUsernameAsync(username, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var userIds = _fixture.CreateMany<Guid>(3).ToList();
        var activeUsers = _fixture.Build<User>()
            .With(u => u.IsActive, true)
            .CreateMany(2)
            .ToList();
            
        activeUsers[0].Id = userIds[0];
        activeUsers[1].Id = userIds[1];
        
        var inactiveUser = _fixture.Build<User>()
            .With(u => u.Id, userIds[2])
            .With(u => u.IsActive, false)
            .Create();
            
        var allUsers = activeUsers.Concat(new[] { inactiveUser }).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(allUsers));

        // Act
        var result = await _repository.GetByIdsAsync(userIds, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(u => u.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetPageAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var activeUsers = _fixture.Build<User>()
            .With(u => u.IsActive, true)
            .CreateMany(5)
            .ToList();
            
        var inactiveUsers = _fixture.Build<User>()
            .With(u => u.IsActive, false)
            .CreateMany(3)
            .ToList();
            
        var allUsers = activeUsers.Concat(inactiveUsers).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(allUsers));

        // Act
        var result = await _repository.GetPageAsync(1, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
        result.All(u => u.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var expectedUser = _fixture.Build<User>()
            .With(u => u.IsActive, true)
            .Create();
        var users = new List<User> { expectedUser };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(users));

        // Act
        var result = await _repository.GetByIdAsync(expectedUser.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedUser.Id);
    }
}

public class TopicRepositoryTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly Mock<StorageSettings> _storageSettingsMock;
    private readonly string _testFilePath;
    private readonly TopicRepository _repository;

    public TopicRepositoryTests()
    {
        _fixture = new Fixture();
            
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_topics_{Guid.NewGuid()}.json");
        
        _storageSettingsMock = new Mock<StorageSettings>();
        _storageSettingsMock.Setup(x => x.StoragePath).Returns(Path.GetTempPath());
        _storageSettingsMock.Setup(x => x.TopicsFileName).Returns(Path.GetFileName(_testFilePath));
        _storageSettingsMock.Setup(x => x.PropertyNamingPolicy).Returns("camelcase");
        _storageSettingsMock.Setup(x => x.WriteIndented).Returns(true);

        _repository = new TopicRepository(_storageSettingsMock.Object);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTopicExists_ReturnsTopic()
    {
        // Arrange
        var expectedTopic = _fixture.Create<Topic>();
        var topics = new List<Topic> { expectedTopic };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(topics));

        // Act
        var result = await _repository.GetByIdAsync(expectedTopic.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedTopic.Id);
    }

    [Fact]
    public async Task FindByNameAsync_WhenTopicExists_ReturnsTopic()
    {
        // Arrange
        var topicName = "Test Topic";
        var topic = _fixture.Build<Topic>()
            .With(t => t.Name, topicName)
            .Create();
            
        var topics = new List<Topic> { topic };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(topics));

        // Act
        var result = await _repository.FindByNameAsync(topicName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(topicName);
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenTopicExists_ReturnsTrue()
    {
        // Arrange
        var topicName = "Existing Topic";
        var topic = _fixture.Build<Topic>()
            .With(t => t.Name, topicName)
            .Create();
            
        var topics = new List<Topic> { topic };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(topics));

        // Act
        var result = await _repository.ExistsByNameAsync(topicName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTopics()
    {
        // Arrange
        var topics = _fixture.CreateMany<Topic>(5).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(topics));

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task AddAsync_AddsTopicToFile()
    {
        // Arrange
        var newTopic = _fixture.Create<Topic>();
        await File.WriteAllTextAsync(_testFilePath, "[]");

        // Act
        var result = await _repository.AddAsync(newTopic);

        // Assert
        result.Should().BeEquivalentTo(newTopic);
        
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var topicsInFile = System.Text.Json.JsonSerializer.Deserialize<List<Topic>>(fileContent);
        topicsInFile.Should().ContainSingle();
        topicsInFile![0].Should().BeEquivalentTo(newTopic);
    }
}

public class QuestionLikeRepositoryTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly Mock<StorageSettings> _storageSettingsMock;
    private readonly string _testFilePath;
    private readonly QuestionLikeRepository _repository;

    public QuestionLikeRepositoryTests()
    {
        _fixture = new Fixture();
            
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_likes_{Guid.NewGuid()}.json");
        
        _storageSettingsMock = new Mock<StorageSettings>();
        _storageSettingsMock.Setup(x => x.StoragePath).Returns(Path.GetTempPath());
        _storageSettingsMock.Setup(x => x.QuestionLikesFileName).Returns(Path.GetFileName(_testFilePath));
        _storageSettingsMock.Setup(x => x.PropertyNamingPolicy).Returns("camelcase");
        _storageSettingsMock.Setup(x => x.WriteIndented).Returns(true);

        _repository = new QuestionLikeRepository(_storageSettingsMock.Object);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public async Task ExistsAsync_WhenLikeExists_ReturnsTrue()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var like = _fixture.Build<QuestionLike>()
            .With(l => l.QuestionId, questionId)
            .With(l => l.UserId, userId)
            .Create();
            
        var likes = new List<QuestionLike> { like };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(likes));

        // Act
        var result = await _repository.ExistsAsync(questionId, userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetCountByQuestionIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var likesForQuestion = _fixture.Build<QuestionLike>()
            .With(l => l.QuestionId, questionId)
            .CreateMany(3)
            .ToList();
            
        var otherLikes = _fixture.Build<QuestionLike>()
            .CreateMany(2)
            .ToList();
            
        var allLikes = likesForQuestion.Concat(otherLikes).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(allLikes));

        // Act
        var result = await _repository.GetCountByQuestionIdAsync(questionId, CancellationToken.None);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task DeleteByQuestionAndUserAsync_RemovesLike()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var likeToRemove = _fixture.Build<QuestionLike>()
            .With(l => l.QuestionId, questionId)
            .With(l => l.UserId, userId)
            .Create();
            
        var otherLike = _fixture.Create<QuestionLike>();
        var likes = new List<QuestionLike> { likeToRemove, otherLike };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(likes));

        // Act
        await _repository.DeleteByQuestionAndUserAsync(questionId, userId, CancellationToken.None);

        // Assert
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var likesInFile = System.Text.Json.JsonSerializer.Deserialize<List<QuestionLike>>(fileContent);
        likesInFile.Should().ContainSingle();
        likesInFile![0].Id.Should().Be(otherLike.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLikeExists_ReturnsLike()
    {
        // Arrange
        var expectedLike = _fixture.Create<QuestionLike>();
        var likes = new List<QuestionLike> { expectedLike };
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(likes));

        // Act
        var result = await _repository.GetByIdAsync(expectedLike.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedLike.Id);
    }

    [Fact]
    public async Task GetByQuestionIdAsync_ReturnsLikesForQuestion()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var likesForQuestion = _fixture.Build<QuestionLike>()
            .With(l => l.QuestionId, questionId)
            .CreateMany(3)
            .ToList();
            
        var otherLikes = _fixture.Build<QuestionLike>()
            .CreateMany(2)
            .ToList();
            
        var allLikes = likesForQuestion.Concat(otherLikes).ToList();
        await File.WriteAllTextAsync(_testFilePath, System.Text.Json.JsonSerializer.Serialize(allLikes));

        // Act
        var result = await _repository.GetByQuestionIdAsync(questionId, CancellationToken.None);

        // Assert
        var questionLikes = result as QuestionLike[] ?? result.ToArray();
        questionLikes.Should().HaveCount(3);
        questionLikes.All(l => l.QuestionId == questionId).Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_AddsLikeToFile()
    {
        // Arrange
        var newLike = _fixture.Build<QuestionLike>()
            .With(l => l.Id, Guid.Empty) // Пустой ID для генерации
            .Create();
        await File.WriteAllTextAsync(_testFilePath, "[]");

        // Act
        var result = await _repository.AddAsync(newLike, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        
        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        var likesInFile = System.Text.Json.JsonSerializer.Deserialize<List<QuestionLike>>(fileContent);
        likesInFile.Should().ContainSingle();
        likesInFile![0].QuestionId.Should().Be(newLike.QuestionId);
        likesInFile![0].UserId.Should().Be(newLike.UserId);
    }
}