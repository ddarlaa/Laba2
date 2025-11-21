using FluentValidation;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.Interfaces;
using IceBreakerApp.Application.IServices;

public class CreateQuestionAnswerValidator : AbstractValidator<CreateQuestionAnswerDTO>
{
    private readonly IQuestionService _questionService;
    private readonly IUserService _userService;

    public CreateQuestionAnswerValidator(IQuestionService questionService, IUserService userService)
    {
        _questionService = questionService;
        _userService = userService;

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required")
            .MustAsync(QuestionExists).WithMessage("Question does not exist");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MustAsync(UserExists).WithMessage("User does not exist");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters");
    }

    private async Task<bool> QuestionExists(Guid questionId, CancellationToken ct)
    {
        var question = await _questionService.GetByIdAsync(questionId, ct);
        return question != null;
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken ct)
    {
        var user = await _userService.GetByIdAsync(userId, ct);
        return user != null;
    }
}