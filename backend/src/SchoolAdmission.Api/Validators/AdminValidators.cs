using FluentValidation;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Validators;

public class VerifyAdminPasswordRequestValidator : AbstractValidator<VerifyAdminPasswordRequest>
{
    public VerifyAdminPasswordRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("The verification password is required.");
    }
}

public class ChangeAdminPasswordRequestValidator : AbstractValidator<ChangeAdminPasswordRequest>
{
    public ChangeAdminPasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("The current verification password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("The new verification password is required.")
            .MinimumLength(8).WithMessage("The new verification password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("The new verification password must be 128 characters or fewer.");
    }
}
