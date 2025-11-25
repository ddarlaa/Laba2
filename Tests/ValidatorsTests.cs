using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentValidation;
using IceBreakerApp.Application.DTOs;
using IceBreakerApp.Application.DTOs.Create;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.Interfaces;
using IceBreakerApp.Application.IServices;

using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IceBreakerApp.Tests;

public class CreateQuestionAnswerValidatorTests
{
    private readonly Mock<IQuestionService> _questionServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly CreateQuestionAnswerValidator _validator;

    public CreateQuestionAnswerValidatorTests()
    {
        _questionServiceMock = new Mock<IQuestionService>();
        _userServiceMock = new Mock<IUserService>();
        _validator = new CreateQuestionAnswerValidator(_questionServiceMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task Validate_EmptyQuestionId_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionAnswerDTO { QuestionId = Guid.Empty, UserId = Guid.NewGuid(), Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Question ID is required");
    }

    [Fact]
    public async Task Validate_EmptyUserId_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionAnswerDTO { QuestionId = Guid.NewGuid(), UserId = Guid.Empty, Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
    }

    [Fact]
    public async Task Validate_EmptyContent_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionAnswerDTO { QuestionId = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content is required");
    }

    [Fact]
    public async Task Validate_ContentTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionAnswerDTO { QuestionId = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "Short" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content must be at least 10 characters");
    }

    [Fact]
    public async Task Validate_ContentTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionAnswerDTO 
        { 
            QuestionId = Guid.NewGuid(), 
            UserId = Guid.NewGuid(), 
            Content = new string('a', 5001) 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content must not exceed 5000 characters");
    }

    [Fact]
    public async Task Validate_QuestionDoesNotExist_ShouldHaveError()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var dto = new CreateQuestionAnswerDTO { QuestionId = questionId, UserId = Guid.NewGuid(), Content = "Valid content" };

        _questionServiceMock.Setup(x => x.GetByIdAsync(questionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionResponseDTO?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Question does not exist");
    }

    [Fact]
    public async Task Validate_UserDoesNotExist_ShouldHaveError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateQuestionAnswerDTO { QuestionId = Guid.NewGuid(), UserId = userId, Content = "Valid content" };

        _questionServiceMock.Setup(x => x.GetByIdAsync(dto.QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionResponseDTO());
        _userServiceMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponseDTO?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "User does not exist");
    }

    [Fact]
    public async Task Validate_ValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateQuestionAnswerDTO 
        { 
            QuestionId = Guid.NewGuid(), 
            UserId = Guid.NewGuid(), 
            Content = "This is a valid answer content with sufficient length" 
        };

        _questionServiceMock.Setup(x => x.GetByIdAsync(dto.QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionResponseDTO());
        _userServiceMock.Setup(x => x.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponseDTO());

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

public class CreateQuestionValidatorTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITopicService> _topicServiceMock;
    private readonly CreateQuestionValidator _validator;

    public CreateQuestionValidatorTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _topicServiceMock = new Mock<ITopicService>();
        _validator = new CreateQuestionValidator(_userServiceMock.Object, _topicServiceMock.Object);
    }

    [Fact]
    public async Task Validate_EmptyUserId_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO { UserId = Guid.Empty, TopicId = Guid.NewGuid(), Title = "Valid Title", Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
    }

    [Fact]
    public async Task Validate_EmptyTopicId_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO { UserId = Guid.NewGuid(), TopicId = Guid.Empty, Title = "Valid Title", Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic ID is required");
    }

    [Fact]
    public async Task Validate_EmptyTitle_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO { UserId = Guid.NewGuid(), TopicId = Guid.NewGuid(), Title = "", Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required");
    }

    [Fact]
    public async Task Validate_TitleTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO { UserId = Guid.NewGuid(), TopicId = Guid.NewGuid(), Title = "abcd", Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title must be at least 5 characters");
    }

    [Fact]
    public async Task Validate_TitleTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO 
        { 
            UserId = Guid.NewGuid(), 
            TopicId = Guid.NewGuid(), 
            Title = new string('a', 201), 
            Content = "Valid content" 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title must not exceed 200 characters");
    }

    [Fact]
    public async Task Validate_EmptyContent_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO { UserId = Guid.NewGuid(), TopicId = Guid.NewGuid(), Title = "Valid Title", Content = "" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content is required");
    }

    [Fact]
    public async Task Validate_ContentTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO { UserId = Guid.NewGuid(), TopicId = Guid.NewGuid(), Title = "Valid Title", Content = "short" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content must be at least 10 characters");
    }

    [Fact]
    public async Task Validate_ContentTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateQuestionDTO 
        { 
            UserId = Guid.NewGuid(), 
            TopicId = Guid.NewGuid(), 
            Title = "Valid Title", 
            Content = new string('a', 5001) 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content must not exceed 5000 characters");
    }

    [Fact]
    public async Task Validate_UserDoesNotExist_ShouldHaveError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateQuestionDTO { UserId = userId, TopicId = Guid.NewGuid(), Title = "Valid Title", Content = "Valid content" };

        _userServiceMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponseDTO?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "User does not exist");
    }

    [Fact]
    public async Task Validate_TopicDoesNotExist_ShouldHaveError()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var dto = new CreateQuestionDTO { UserId = Guid.NewGuid(), TopicId = topicId, Title = "Valid Title", Content = "Valid content" };

        _userServiceMock.Setup(x => x.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponseDTO());
        _topicServiceMock.Setup(x => x.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TopicResponseDTO?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic does not exist");
    }

    [Fact]
    public async Task Validate_ValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateQuestionDTO 
        { 
            UserId = Guid.NewGuid(), 
            TopicId = Guid.NewGuid(), 
            Title = "Valid Question Title", 
            Content = "This is valid question content with sufficient length" 
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponseDTO());
        _topicServiceMock.Setup(x => x.GetByIdAsync(dto.TopicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TopicResponseDTO(Guid.NewGuid(), "Topic", "Description", DateTime.UtcNow));

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

public class CreateTopicValidatorTests
{
    private readonly Mock<ITopicService> _topicServiceMock;
    private readonly CreateTopicValidator _validator;

    public CreateTopicValidatorTests()
    {
        _topicServiceMock = new Mock<ITopicService>();
        _validator = new CreateTopicValidator(_topicServiceMock.Object);
    }

    [Fact]
    public async Task Validate_EmptyName_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = "", Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name is required");
    }

    [Fact]
    public async Task Validate_NameTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = "a", Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name must be between 2 and 100 characters");
    }

    [Fact]
    public async Task Validate_NameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = new string('a', 101), Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name must be between 2 and 100 characters");
    }

    [Theory]
    [InlineData("Invalid@Name")]
    [InlineData("Invalid#Name")]
    [InlineData("Invalid$Name")]
    [InlineData("Invalid&Name")]
    public async Task Validate_NameWithInvalidCharacters_ShouldHaveError(string name)
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = name, Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name can only contain letters, numbers, spaces, hyphens and dots");
    }

    [Theory]
    [InlineData("Valid Topic Name")]
    [InlineData("Valid-Topic-Name")]
    [InlineData("Valid.Topic.Name")]
    [InlineData("Valid123")]
    public async Task Validate_NameWithValidCharacters_ShouldPass(string name)
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = name, Description = "Valid description" };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NameAlreadyExists_ShouldHaveError()
    {
        // Arrange
        var name = "Existing Topic";
        var dto = new CreateTopicDTO { Name = name, Description = "Valid description" };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name already exists");
    }

    [Fact]
    public async Task Validate_DescriptionExceedsLimit_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = "Valid Topic", Description = new string('a', 501) };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Description must not exceed 500 characters");
    }

    [Fact]
    public async Task Validate_NullDescription_ShouldPass()
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = "Valid Topic", Description = null };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync("Valid Topic", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateTopicDTO { Name = "Valid Topic", Description = "Valid description" };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync("Valid Topic", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

public class CreateUserValidatorTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly CreateUserValidator _validator;

    public CreateUserValidatorTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _validator = new CreateUserValidator(_userServiceMock.Object);
    }

    [Fact]
    public async Task Validate_EmptyUsername_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "", Email = "test@example.com", Password = "ValidPass123", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Username is required");
    }

    [Fact]
    public async Task Validate_UsernameTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "ab", Email = "test@example.com", Password = "ValidPass123", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Username must be between 3 and 50 characters");
    }

    [Fact]
    public async Task Validate_UsernameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO 
        { 
            Username = new string('a', 51), 
            Email = "test@example.com", 
            Password = "ValidPass123", 
            DisplayName = "Valid Name" 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Username must be between 3 and 50 characters");
    }

    [Theory]
    [InlineData("Invalid@Username")]
    [InlineData("Invalid#Username")]
    [InlineData("Invalid Username")]
    [InlineData("Invalid-Username")]
    [InlineData("Invalid.Username")]
    public async Task Validate_UsernameWithInvalidCharacters_ShouldHaveError(string username)
    {
        // Arrange
        var dto = new CreateUserDTO { Username = username, Email = "test@example.com", Password = "ValidPass123", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Username can only contain letters, numbers and underscores");
    }

    [Fact]
    public async Task Validate_UsernameAlreadyExists_ShouldHaveError()
    {
        // Arrange
        var username = "existinguser";
        var dto = new CreateUserDTO { Username = username, Email = "test@example.com", Password = "ValidPass123", DisplayName = "Valid Name" };

        _userServiceMock.Setup(x => x.FindByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponseDTO());

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Username already exists");
    }

    [Fact]
    public async Task Validate_EmptyEmail_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "", Password = "ValidPass123", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public async Task Validate_InvalidEmailFormat_ShouldHaveError(string email)
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = email, Password = "ValidPass123", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid email format");
    }

    [Fact]
    public async Task Validate_EmailAlreadyExists_ShouldHaveError()
    {
        // Arrange
        var email = "existing@example.com";
        var dto = new CreateUserDTO { Username = "validuser", Email = email, Password = "ValidPass123", DisplayName = "Valid Name" };

        _userServiceMock.Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponseDTO());

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email already exists");
    }

    [Fact]
    public async Task Validate_EmptyPassword_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = "", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }

    [Fact]
    public async Task Validate_PasswordTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = "12345", DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters");
    }

    [Fact]
    public async Task Validate_PasswordTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO 
        { 
            Username = "validuser", 
            Email = "test@example.com", 
            Password = new string('a', 101), 
            DisplayName = "Valid Name" 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must not exceed 100 characters");
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("passwordABC")]
    [InlineData("Password")]
    public async Task Validate_PasswordWithoutUppercase_ShouldHaveError(string password)
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = password, DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one uppercase letter");
    }

    [Theory]
    [InlineData("PASSWORD123")]
    [InlineData("PASSWORD")]
    [InlineData("Password123")]
    public async Task Validate_PasswordWithoutLowercase_ShouldHaveError(string password)
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = password, DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one lowercase letter");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("PASSWORD")]
    [InlineData("Password")]
    public async Task Validate_PasswordWithoutNumber_ShouldHaveError(string password)
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = password, DisplayName = "Valid Name" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one number");
    }

    [Fact]
    public async Task Validate_EmptyDisplayName_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = "ValidPass123", DisplayName = "" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name is required");
    }

    [Fact]
    public async Task Validate_DisplayNameTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO { Username = "validuser", Email = "test@example.com", Password = "ValidPass123", DisplayName = "a" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name must be between 2 and 100 characters");
    }

    [Fact]
    public async Task Validate_DisplayNameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO 
        { 
            Username = "validuser", 
            Email = "test@example.com", 
            Password = "ValidPass123", 
            DisplayName = new string('a', 101) 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name must be between 2 and 100 characters");
    }

    [Fact]
    public async Task Validate_BioExceedsLimit_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateUserDTO 
        { 
            Username = "validuser", 
            Email = "test@example.com", 
            Password = "ValidPass123", 
            DisplayName = "Valid Name", 
            Bio = new string('a', 501) 
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Bio must not exceed 500 characters");
    }

    [Fact]
    public async Task Validate_NullBio_ShouldPass()
    {
        // Arrange
        var dto = new CreateUserDTO 
        { 
            Username = "validuser", 
            Email = "test@example.com", 
            Password = "ValidPass123", 
            DisplayName = "Valid Name", 
            Bio = null 
        };

        _userServiceMock.Setup(x => x.FindByUsernameAsync("validuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponseDTO?)null);
        _userServiceMock.Setup(x => x.FindByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponseDTO?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateUserDTO 
        { 
            Username = "validuser", 
            Email = "test@example.com", 
            Password = "ValidPass123", 
            DisplayName = "Valid Name", 
            Bio = "This is a valid bio" 
        };

        _userServiceMock.Setup(x => x.FindByUsernameAsync("validuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponseDTO?)null);
        _userServiceMock.Setup(x => x.FindByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponseDTO?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

public class UpdateQuestionValidatorTests
{
    private readonly UpdateQuestionValidator _validator;

    public UpdateQuestionValidatorTests()
    {
        _validator = new UpdateQuestionValidator();
    }

    [Fact]
    public async Task Validate_EmptyTitle_ShouldPassBecauseOfWhenCondition()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "", Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        // Empty string with When condition should pass because When prevents validation
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_TitleTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "abcd", Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title must be at least 5 characters");
    }

    [Fact]
    public async Task Validate_TitleTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = new string('a', 201), Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title must not exceed 200 characters");
    }

    [Fact]
    public async Task Validate_EmptyContent_ShouldPassBecauseOfWhenCondition()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "Valid Title", Content = "" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        // Empty string with When condition should pass because When prevents validation
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ContentTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "Valid Title", Content = "short" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content must be at least 10 characters");
    }

    [Fact]
    public async Task Validate_ContentTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "Valid Title", Content = new string('a', 5001) };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content must not exceed 5000 characters");
    }

    [Fact]
    public async Task Validate_NullTitle_ShouldPass()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = null, Content = "Valid content" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NullContent_ShouldPass()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "Valid Title", Content = null };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_BothValidTitleAndContent_ShouldPass()
    {
        // Arrange
        var dto = new UpdateQuestionDTO { Title = "Valid Title", Content = "Valid content with sufficient length" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

public class UpdateTopicValidatorTests
{
    private readonly Mock<ITopicService> _topicServiceMock;
    private readonly UpdateTopicValidator _validator;

    public UpdateTopicValidatorTests()
    {
        _topicServiceMock = new Mock<ITopicService>();
        _validator = new UpdateTopicValidator(_topicServiceMock.Object);
    }

    [Fact]
    public async Task Validate_EmptyName_ShouldPassBecauseOfWhenCondition()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = "", Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        // Empty string with When condition should pass because When prevents validation
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NameTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = "a", Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name must be between 2 and 100 characters");
    }

    [Fact]
    public async Task Validate_NameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = new string('a', 101), Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name must be between 2 and 100 characters");
    }

    [Theory]
    [InlineData("Invalid@Name")]
    [InlineData("Invalid#Name")]
    [InlineData("Invalid$Name")]
    public async Task Validate_NameWithInvalidCharacters_ShouldHaveError(string name)
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = name, Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name can only contain letters, numbers, spaces, hyphens and dots");
    }

    [Theory]
    [InlineData("Valid Topic Name")]
    [InlineData("Valid-Topic-Name")]
    [InlineData("Valid.Topic.Name")]
    [InlineData("Valid123")]
    public async Task Validate_NameWithValidCharacters_ShouldPass(string name)
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = name, Description = "Valid description" };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NameAlreadyExists_ShouldHaveError()
    {
        // Arrange
        var name = "Existing Topic";
        var dto = new UpdateTopicDTO { Name = name, Description = "Valid description" };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Topic name already exists");
    }

    [Fact]
    public async Task Validate_NullName_ShouldPass()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = null, Description = "Valid description" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_DescriptionExceedsLimit_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = "Valid Topic", Description = new string('a', 501) };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Description must not exceed 500 characters");
    }

    [Fact]
    public async Task Validate_NullDescription_ShouldPass()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = "Valid Topic", Description = null };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync("Valid Topic", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_BothValidNameAndDescription_ShouldPass()
    {
        // Arrange
        var dto = new UpdateTopicDTO { Name = "Valid Topic", Description = "Valid description" };

        _topicServiceMock.Setup(x => x.ExistsByNameAsync("Valid Topic", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

public class UpdateUserValidatorTests
{
    private readonly UpdateUserValidator _validator;

    public UpdateUserValidatorTests()
    {
        _validator = new UpdateUserValidator();
    }

    [Fact]
    public async Task Validate_EmptyDisplayName_ShouldPassBecauseOfWhenCondition()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = "", Bio = "Valid bio" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        // Empty string with When condition should pass because When prevents validation
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_DisplayNameTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = "a", Bio = "Valid bio" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name must be between 2 and 100 characters");
    }

    [Fact]
    public async Task Validate_DisplayNameTooLong_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = new string('a', 101), Bio = "Valid bio" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name must be between 2 and 100 characters");
    }

    [Fact]
    public async Task Validate_NullDisplayName_ShouldPass()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = null, Bio = "Valid bio" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_BioExceedsLimit_ShouldHaveError()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = "Valid Name", Bio = new string('a', 501) };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Bio must not exceed 500 characters");
    }

    [Fact]
    public async Task Validate_NullBio_ShouldPass()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = "Valid Name", Bio = null };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_BothValidDisplayNameAndBio_ShouldPass()
    {
        // Arrange
        var dto = new UpdateUserDTO { DisplayName = "Valid Name", Bio = "Valid bio" };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}