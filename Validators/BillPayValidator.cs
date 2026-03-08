using FluentValidation;
using HospitalApi.Dtos;

namespace HospitalApi.Validators
{
    public class BillPayValidator : AbstractValidator<BillPayDto>
    {
        public BillPayValidator()
        {
            RuleFor(x => x.PaidAmount)
                .GreaterThan(0).WithMessage("Paid amount must be greater than zero.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Invalid payment method.");

            RuleFor(x => x.TransactionReference)
                .MaximumLength(100).WithMessage("Transaction reference cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.TransactionReference));
        }
    }
}