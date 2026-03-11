using FluentValidation;
using HospitalApi.Dtos;

namespace HospitalApi.Validators
{
    public class AppointmentCreateValidator : AbstractValidator<AppointmentCreateDto>
    {
        public AppointmentCreateValidator()
        {
            RuleFor(x => x.PatientId)
                .GreaterThan(0).WithMessage("A valid Patient is required.");

            RuleFor(x => x.DoctorId)
                .GreaterThan(0).WithMessage("A valid Doctor is required.");

            RuleFor(x => x.AppointmentDate)
                .NotEmpty().WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}