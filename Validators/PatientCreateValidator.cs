using FluentValidation;
using HospitalApi.Dtos;

public class PatientCreateValidator : AbstractValidator<PatientCreateDto>
{
    public PatientCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120);

        RuleFor(x => x.Disease)
            .MaximumLength(200);
    }
}