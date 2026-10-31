namespace Bookify.Contracts.Polls;

public class CreatePollRequestValidator:AbstractValidator<PollRequest>
{
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(3, 100)
            .WithMessage("{PropertyName} must be between [{MinLength}] and [{MaxLength}] characters");
        RuleFor(x => x.Summary)
            .NotEmpty()
            .Length(3, 250)
            .WithMessage("{PropertyName} must be between [{MinLength}] and [{MaxLength}] characters");
        RuleFor(x => x.StartsAt)
    .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
    .WithMessage("The start date must be today or in the future.");
        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt).WithMessage("End date must be greater than start date");
    }
}
