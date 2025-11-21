using FluentValidation;
using IceBreakerApp.Application.DTOs.Response;
using IceBreakerApp.Application.IServices;

public class CreateTopicValidator : AbstractValidator<CreateTopicDTO>
{
    private readonly ITopicService _topicService;

    public CreateTopicValidator(ITopicService topicService)
    {
        _topicService = topicService;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Topic name is required")
            .Length(2, 100).WithMessage("Topic name must be between 2 and 100 characters")
            .Matches("^[a-zA-Z0-9\\s\\-\\.]+$").WithMessage("Topic name can only contain letters, numbers, spaces, hyphens and dots")
            .MustAsync(BeUniqueName).WithMessage("Topic name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
    {
        return !await _topicService.ExistsByNameAsync(name, ct);
    }
}