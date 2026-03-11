using FluentValidation;
using HospitalApi.Dtos;

namespace HospitalApi.Validators
{
    public class BillCreateValidator : AbstractValidator<BillCreateDto>
    {
        public BillCreateValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0).WithMessage("A valid Appointment is required.");

            RuleFor(x => x.ConsultationFee)
                .GreaterThanOrEqualTo(0).WithMessage("Consultation fee cannot be negative.");

            RuleFor(x => x.MedicineFee)
                .GreaterThanOrEqualTo(0).WithMessage("Medicine fee cannot be negative.");

            RuleFor(x => x.OtherCharges)
                .GreaterThanOrEqualTo(0).WithMessage("Other charges cannot be negative.");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}