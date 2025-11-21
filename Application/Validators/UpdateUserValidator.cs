using FluentValidation;
using IceBreakerApp.Application.DTOs.Update;

public class UpdateUserValidator : AbstractValidator<UpdateUserDTO>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name cannot be empty")
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .Length(2, 100).WithMessage("Display name must be between 2 and 100 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}