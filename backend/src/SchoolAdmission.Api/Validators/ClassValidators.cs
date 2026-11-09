using FluentValidation;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Validators;

public class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Class name is required.")
            .MaximumLength(200).WithMessage("Class name must be 200 characters or fewer.");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("Total seats must be greater than zero.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Total seats is unrealistically large.");
    }
}
