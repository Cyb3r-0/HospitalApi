using FluentValidation;
using HospitalApi.Dtos;

namespace HospitalApi.Validators
{
    public class DoctorCreateValidator : AbstractValidator<DoctorCreateDto>
    {
        public DoctorCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Specialization)
                .NotEmpty().WithMessage("Specialization is required.")
                .MaximumLength(100).WithMessage("Specialization cannot exceed 100 characters.");

            RuleFor(x => x.Phone)
                .MaximumLength(15).WithMessage("Phone cannot exceed 15 characters.")
                .Matches(@"^\+?[0-9\s\-]+$").WithMessage("Phone number is invalid.")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email is not valid.")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
