using FluentValidation;
using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Validators
{
    public class AppointmentUpdateValidator : AbstractValidator<AppointmentUpdateDto>
    {
        public AppointmentUpdateValidator()
        {
            RuleFor(x => x.AppointmentDate)
                .NotEmpty().WithMessage("Appointment date is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future.")
                .When(x => x.Status == AppointmentStatus.Scheduled);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value. Use 1=Scheduled, 2=Completed, 3=Cancelled.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}