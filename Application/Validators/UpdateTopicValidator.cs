using FluentValidation;
using IceBreakerApp.Application.DTOs.Update;
using IceBreakerApp.Application.IServices;

public class UpdateTopicValidator : AbstractValidator<UpdateTopicDTO>
{
    private readonly ITopicService _topicService;

    public UpdateTopicValidator(ITopicService topicService)
    {
        _topicService = topicService;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Topic name cannot be empty")
            .When(x => !string.IsNullOrEmpty(x.Name))
            .Length(2, 100).WithMessage("Topic name must be between 2 and 100 characters")
            .Matches("^[a-zA-Z0-9\\s\\-\\.]+$").WithMessage("Topic name can only contain letters, numbers, spaces, hyphens and dots")
            .MustAsync(BeUniqueName).WithMessage("Topic name already exists")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
    {
        return !await _topicService.ExistsByNameAsync(name, ct);
    }
}