using FluentValidation;
using IceBreakerApp.Application.DTOs.Create;
using IceBreakerApp.Application.DTOs.Response;

public class CreateUserValidator : AbstractValidator<CreateUserDTO>
{
    private readonly IUserService _userService;

    public CreateUserValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores")
            .MustAsync(BeUniqueUsername).WithMessage("Username already exists");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .Length(2, 100).WithMessage("Display name must be between 2 and 100 characters");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken ct)
    {
        var existingUser = await _userService.FindByUsernameAsync(username, ct);
        return existingUser == null;
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
    {
        var existingUser = await _userService.FindByEmailAsync(email, ct);
        return existingUser == null;
    }
}