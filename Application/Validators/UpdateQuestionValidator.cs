using FluentValidation;
using IceBreakerApp.Application.DTOs.Update;

public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionDTO>
{
    public UpdateQuestionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty")
            .When(x => !string.IsNullOrEmpty(x.Title))
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content cannot be empty")
            .When(x => !string.IsNullOrEmpty(x.Content))
            .MinimumLength(10).WithMessage("Content must be at least 10 characters")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Content));
    }
}