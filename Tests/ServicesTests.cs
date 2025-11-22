using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Create;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.Services;
using IceBreakerApp.Domain;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Domain.IRepositories;
using IceBreakerApp.Domain.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Moq;

namespace IceBreakerApp.Tests;

public class QuestionServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITopicRepository> _topicRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly QuestionService _service;

    public QuestionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _topicRepositoryMock = new Mock<ITopicRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new QuestionService(
            _questionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _topicRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingActiveQuestion_ShouldReturnDTO()
    {
        // Arrange
        var question = _fixture.Create<Question>();
        var responseDto = _fixture.Create<QuestionResponseDTO>();

        _questionRepositoryMock.Setup(x => x.GetByIdAsync(question.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        _mapperMock.Setup(x => x.Map<QuestionResponseDTO>(question))
            .Returns(responseDto);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(question.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<User>());
        _topicRepositoryMock.Setup(x => x.GetByIdAsync(question.TopicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<Topic>());

        // Act
        var result = await _service.GetByIdAsync(question.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
        _questionRepositoryMock.Verify(x => x.UpdateAsync(question, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingQuestion_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        _questionRepositoryMock.Setup(x => x.GetByIdAsync(nonExistingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_InactiveQuestion_ShouldReturnNull()
    {
        // Arrange
        var inactiveQuestion = _fixture.Build<Question>().With(q => q.IsActive, false).Create();

        _questionRepositoryMock.Setup(x => x.GetByIdAsync(inactiveQuestion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveQuestion);

        // Act
        var result = await _service.GetByIdAsync(inactiveQuestion.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var questions = _fixture.CreateMany<Question>(10).ToList();
        var paginatedResult = new PaginatedResult<Question>(questions, 10, 1, 5);
        var responseDtos = _fixture.CreateMany<QuestionResponseDTO>(10).ToList();

        _questionRepositoryMock.Setup(x => x.GetPaginatedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), 
            It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);
        
        _userRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.CreateMany<User>(3).ToList());
        _topicRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.CreateMany<Topic>(3).ToList());

        _mapperMock.Setup(x => x.Map<List<QuestionResponseDTO>>(questions))
            .Returns(responseDtos);

        // Act
        var result = await _service.GetAllAsync(1, 5, null, null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ShouldCreateAndReturnDTO()
    {
        // Arrange
        var createDto = _fixture.Create<CreateQuestionDTO>();
        var user = _fixture.Create<User>();
        var topic = _fixture.Create<Topic>();
        var question = _fixture.Create<Question>();
        var responseDto = _fixture.Create<QuestionResponseDTO>();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(createDto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _topicRepositoryMock.Setup(x => x.GetByIdAsync(createDto.TopicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _questionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        _mapperMock.Setup(x => x.Map<QuestionResponseDTO>(question))
            .Returns(responseDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(createDto.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _topicRepositoryMock.Verify(x => x.GetByIdAsync(createDto.TopicId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NonExistingUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var createDto = _fixture.Create<CreateQuestionDTO>();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(createDto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateAsync(createDto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_NonExistingTopic_ShouldThrowNotFoundException()
    {
        // Arrange
        var createDto = _fixture.Create<CreateQuestionDTO>();
        var user = _fixture.Create<User>();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(createDto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _topicRepositoryMock.Setup(x => x.GetByIdAsync(createDto.TopicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateAsync(createDto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ExistingQuestion_ShouldUpdateAndNotThrow()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var updateDto = _fixture.Create<UpdateQuestionDTO>();
        var question = _fixture.Create<Question>();
        var topic = _fixture.Create<Topic>();

        _questionRepositoryMock.Setup(x => x.GetByIdAsync(questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        _topicRepositoryMock.Setup(x => x.GetByIdAsync(updateDto.TopicId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);

        // Act & Assert
        await FluentActions.Invoking(() => _service.UpdateAsync(questionId, updateDto))
            .Should().NotThrowAsync();

        _questionRepositoryMock.Verify(x => x.UpdateAsync(question, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingQuestion_ShouldSoftDelete()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var question = _fixture.Create<Question>();

        _questionRepositoryMock.Setup(x => x.GetByIdAsync(questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        // Act
        await _service.DeleteAsync(questionId);

        // Assert
        question.IsActive.Should().BeFalse();
        _questionRepositoryMock.Verify(x => x.UpdateAsync(question, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkCreateAsync_MultipleValidDtos_ShouldCreateAll()
    {
        // Arrange
        var createDtos = _fixture.CreateMany<CreateQuestionDTO>(3).ToList();
        var users = _fixture.CreateMany<User>(3).ToList();
        var topics = _fixture.CreateMany<Topic>(3).ToList();
        var questions = _fixture.CreateMany<Question>(3).ToList();
        var responseDtos = _fixture.CreateMany<QuestionResponseDTO>(3).ToList();

        for (int i = 0; i < 3; i++)
        {
            var i1 = i;
            _userRepositoryMock.Setup(x => x.GetByIdAsync(createDtos[i1].UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(users[i]);
            var i2 = i;
            _topicRepositoryMock.Setup(x => x.GetByIdAsync(createDtos[i2].TopicId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(topics[i]);
            _questionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(questions[i]);
        }

        _mapperMock.Setup(x => x.Map<QuestionResponseDTO>(It.IsAny<Question>()))
            .Returns((Question q) => responseDtos[questions.IndexOf(q)]);

        // Act
        var result = await _service.BulkCreateAsync(createDtos);

        // Assert
        result.SuccessItems.Should().HaveCount(3);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task PatchAsync_ValidPatch_ShouldApplyChanges()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var question = _fixture.Create<Question>();
        var patchDoc = new JsonPatchDocument<UpdateQuestionDTO>();
        patchDoc.Replace(x => x.Title, "New Title");

        _questionRepositoryMock.Setup(x => x.GetByIdAsync(questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);

        // Act
        await _service.PatchAsync(questionId, patchDoc);

        // Assert
        _questionRepositoryMock.Verify(x => x.UpdateAsync(question, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class QuestionAnswerServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IQuestionAnswerRepository> _answerRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly QuestionAnswerService _service;

    public QuestionAnswerServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _answerRepositoryMock = new Mock<IQuestionAnswerRepository>();
        _mapperMock = new Mock<IMapper>();

        _service = new QuestionAnswerService(_answerRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAnswer_ShouldReturnDTO()
    {
        // Arrange
        var answer = _fixture.Create<QuestionAnswer>();
        var responseDto = _fixture.Create<QuestionAnswerResponseDTO>();

        _answerRepositoryMock.Setup(x => x.GetByIdAsync(answer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);
        _mapperMock.Setup(x => x.Map<QuestionAnswerResponseDTO>(answer))
            .Returns(responseDto);

        // Act
        var result = await _service.GetByIdAsync(answer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
        _answerRepositoryMock.Verify(x => x.UpdateAsync(answer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ShouldCreateAndReturnDTO()
    {
        // Arrange
        var createDto = _fixture.Create<CreateQuestionAnswerDTO>();
        var answer = _fixture.Create<QuestionAnswer>();
        var responseDto = _fixture.Create<QuestionAnswerResponseDTO>();

        _mapperMock.Setup(x => x.Map<QuestionAnswer>(createDto))
            .Returns(answer);
        _answerRepositoryMock.Setup(x => x.AddAsync(answer, It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);
        _mapperMock.Setup(x => x.Map<QuestionAnswerResponseDTO>(answer))
            .Returns(responseDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
        _answerRepositoryMock.Verify(x => x.AddAsync(answer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkCreateAsync_MultipleValidDtos_ShouldCreateAll()
    {
        // Arrange
        var createDtos = _fixture.CreateMany<CreateQuestionAnswerDTO>(3).ToList();
        var answers = _fixture.CreateMany<QuestionAnswer>(3).ToList();
        var responseDtos = _fixture.CreateMany<QuestionAnswerResponseDTO>(3).ToList();

        _mapperMock.Setup(x => x.Map<List<QuestionAnswer>>(createDtos))
            .Returns(answers);
        _answerRepositoryMock.Setup(x => x.AddBulkAsync(answers, It.IsAny<CancellationToken>()))
            .ReturnsAsync(answers);
        _mapperMock.Setup(x => x.Map<List<QuestionAnswerResponseDTO>>(answers))
            .Returns(responseDtos);

        // Act
        var result = await _service.BulkCreateAsync(createDtos);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(responseDtos);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAnswer_ShouldUpdateContent()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var updateDto = _fixture.Create<UpdateQuestionAnswerDTO>();
        var answer = _fixture.Create<QuestionAnswer>();

        _answerRepositoryMock.Setup(x => x.GetByIdAsync(answerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);

        // Act
        await _service.UpdateAsync(answerId, updateDto);

        // Assert
        answer.Content.Should().Be(updateDto.Content);
        _answerRepositoryMock.Verify(x => x.UpdateAsync(answer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAnswer_ShouldSoftDelete()
    {
        // Arrange
        var answerId = Guid.NewGuid();
        var answer = _fixture.Create<QuestionAnswer>();

        _answerRepositoryMock.Setup(x => x.GetByIdAsync(answerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(answer);

        // Act
        await _service.DeleteAsync(answerId);

        // Assert
        answer.IsActive.Should().BeFalse();
        _answerRepositoryMock.Verify(x => x.UpdateAsync(answer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_ValidAnswerId_ShouldMarkAsAccepted()
    {
        // Arrange
        var answerId = Guid.NewGuid();

        // Act
        await _service.AcceptAsync(answerId);

        // Assert
        _answerRepositoryMock.Verify(x => x.MarkAsAcceptedAsync(answerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class QuestionLikeServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IQuestionLikeRepository> _likeRepositoryMock;
    private readonly QuestionLikeService _service;

    public QuestionLikeServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _likeRepositoryMock = new Mock<IQuestionLikeRepository>();
        var mapperMock = new Mock<IMapper>();

        _service = new QuestionLikeService(_likeRepositoryMock.Object, mapperMock.Object);
    }

    [Fact]
    public async Task AddLikeAsync_NotExistingLike_ShouldAddLikeAndReturnTrue()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.ExistsAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.AddLikeAsync(questionId, userId);

        // Assert
        result.Should().BeTrue();
        _likeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<QuestionLike>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddLikeAsync_ExistingLike_ShouldReturnFalse()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.ExistsAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AddLikeAsync(questionId, userId);

        // Assert
        result.Should().BeFalse();
        _likeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<QuestionLike>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveLikeAsync_ExistingLike_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.ExistsAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RemoveLikeAsync(questionId, userId);

        // Assert
        result.Should().BeTrue();
        _likeRepositoryMock.Verify(x => x.DeleteByQuestionAndUserAsync(questionId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveLikeAsync_NonExistingLike_ShouldReturnFalse()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.ExistsAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.RemoveLikeAsync(questionId, userId);

        // Assert
        result.Should().BeFalse();
        _likeRepositoryMock.Verify(x => x.DeleteByQuestionAndUserAsync(questionId, userId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetLikeCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var expectedCount = 5;

        _likeRepositoryMock.Setup(x => x.GetCountByQuestionIdAsync(questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetLikeCountAsync(questionId);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task HasUserLikedAsync_ExistingLike_ShouldReturnTrue()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _likeRepositoryMock.Setup(x => x.ExistsAsync(questionId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.HasUserLikedAsync(questionId, userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserLikedQuestionIdsAsync_ShouldReturnUserLikedQuestions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var likes = _fixture.CreateMany<QuestionLike>(3).ToList();
        var expectedIds = likes.Select(l => l.QuestionId).ToList();

        _likeRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(likes);

        // Act
        var result = await _service.GetUserLikedQuestionIdsAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedIds);
    }
}

public class TopicServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ITopicRepository> _topicRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TopicService _service;

    public TopicServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _topicRepositoryMock = new Mock<ITopicRepository>();
        _mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<TopicService>>();

        _service = new TopicService(_topicRepositoryMock.Object, _mapperMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTopic_ShouldReturnDTO()
    {
        // Arrange
        var topic = _fixture.Create<Topic>();
        var responseDto = _fixture.Create<TopicResponseDTO>();

        _topicRepositoryMock.Setup(x => x.GetByIdAsync(topic.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _mapperMock.Setup(x => x.Map<TopicResponseDTO>(topic))
            .Returns(responseDto);

        // Act
        var result = await _service.GetByIdAsync(topic.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTopic_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        _topicRepositoryMock.Setup(x => x.GetByIdAsync(nonExistingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        // Act
        var result = await _service.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var topics = _fixture.CreateMany<Topic>(10).ToList();
        var paginatedResult = new PaginatedResult<Topic>(topics, 10, 1, 5);
        var responseDtos = _fixture.CreateMany<TopicResponseDTO>(10).ToList();

        _topicRepositoryMock.Setup(x => x.GetPaginatedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);
        _mapperMock.Setup(x => x.Map<List<TopicResponseDTO>>(topics))
            .Returns(responseDtos);

        // Act
        var result = await _service.GetAllAsync(1, 5);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ShouldCreateAndReturnDTO()
    {
        // Arrange
        var createDto = _fixture.Create<CreateTopicDTO>();
        var topic = _fixture.Create<Topic>();
        var responseDto = _fixture.Create<TopicResponseDTO>();

        _topicRepositoryMock.Setup(x => x.FindByNameAsync(createDto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);
        _mapperMock.Setup(x => x.Map<Topic>(createDto))
            .Returns(topic);
        _topicRepositoryMock.Setup(x => x.AddAsync(topic, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _mapperMock.Setup(x => x.Map<TopicResponseDTO>(topic))
            .Returns(responseDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
        _topicRepositoryMock.Verify(x => x.AddAsync(topic, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ExistingName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createDto = _fixture.Create<CreateTopicDTO>();
        var existingTopic = _fixture.Create<Topic>();

        _topicRepositoryMock.Setup(x => x.FindByNameAsync(createDto.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTopic);

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateAsync(createDto))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_ExistingTopic_ShouldUpdateAndNotThrow()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var updateDto = _fixture.Create<UpdateTopicDTO>();
        var topic = _fixture.Create<Topic>();

        _topicRepositoryMock.Setup(x => x.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _topicRepositoryMock.Setup(x => x.FindByNameAsync(updateDto.Name!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        // Act & Assert
        await FluentActions.Invoking(() => _service.UpdateAsync(topicId, updateDto))
            .Should().NotThrowAsync();

        _topicRepositoryMock.Verify(x => x.UpdateAsync(topic, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTopic_ShouldDelete()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var topic = _fixture.Create<Topic>();

        _topicRepositoryMock.Setup(x => x.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);

        // Act
        await _service.DeleteAsync(topicId);

        // Assert
        _topicRepositoryMock.Verify(x => x.DeleteAsync(topicId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ShouldReturnTrue()
    {
        // Arrange
        var topicName = "Existing Topic";
        var topic = _fixture.Create<Topic>();

        _topicRepositoryMock.Setup(x => x.FindByNameAsync(topicName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);

        // Act
        var result = await _service.ExistsByNameAsync(topicName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ShouldReturnFalse()
    {
        // Arrange
        var topicName = "Non-existing Topic";

        _topicRepositoryMock.Setup(x => x.FindByNameAsync(topicName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        // Act
        var result = await _service.ExistsByNameAsync(topicName);

        // Assert
        result.Should().BeFalse();
    }
}

public class UserServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<UserService>>();
        _mapperMock = new Mock<IMapper>();

        _service = new UserService(_userRepositoryMock.Object, loggerMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnDTO()
    {
        // Arrange
        var user = _fixture.Create<User>();
        var responseDto = _fixture.Create<UserResponseDTO>();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponseDTO>(user))
            .Returns(responseDto);

        // Act
        var result = await _service.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var users = _fixture.CreateMany<User>(10).ToList();
        var responseDtos = _fixture.CreateMany<UserResponseDTO>(10).ToList();

        _userRepositoryMock.Setup(x => x.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        _mapperMock.Setup(x => x.Map<List<UserResponseDTO>>(users))
            .Returns(responseDtos);

        // Act
        var result = await _service.GetAllAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ShouldCreateAndReturnDTO()
    {
        // Arrange
        var createDto = _fixture.Create<CreateUserDTO>();
        var user = _fixture.Create<User>();
        var responseDto = _fixture.Create<UserResponseDTO>();

        _mapperMock.Setup(x => x.Map<User>(createDto))
            .Returns(user);
        _userRepositoryMock.Setup(x => x.AddAsync(user, It.IsAny<CancellationToken>()))
            .Verifiable();
        _mapperMock.Setup(x => x.Map<UserResponseDTO>(user))
            .Returns(responseDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
        _userRepositoryMock.Verify(x => x.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_ShouldUpdateFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = _fixture.Create<UpdateUserDTO>();
        var user = _fixture.Create<User>();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _service.UpdateAsync(userId, updateDto);

        // Assert
        user.DisplayName.Should().Be(updateDto.DisplayName);
        user.Bio.Should().Be(updateDto.Bio);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ShouldDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _service.DeleteAsync(userId);

        // Assert
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FindByEmailAsync_ExistingUser_ShouldReturnDTO()
    {
        // Arrange
        var email = "test@example.com";
        var user = _fixture.Create<User>();
        var responseDto = _fixture.Create<UserResponseDTO>();

        _userRepositoryMock.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponseDTO>(user))
            .Returns(responseDto);

        // Act
        var result = await _service.FindByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
    }

    [Fact]
    public async Task FindByUsernameAsync_ExistingUser_ShouldReturnDTO()
    {
        // Arrange
        var username = "testuser";
        var user = _fixture.Create<User>();
        var responseDto = _fixture.Create<UserResponseDTO>();

        _userRepositoryMock.Setup(x => x.FindByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserResponseDTO>(user))
            .Returns(responseDto);

        // Act
        var result = await _service.FindByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(responseDto);
    }
}
