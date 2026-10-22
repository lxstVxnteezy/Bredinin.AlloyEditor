using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.MechanicalProperty;

public class CreateMechanicalPropertyRequestValidator : AbstractValidator<CreateMechanicalPropertyRequest>
{
    public CreateMechanicalPropertyRequestValidator()
    {
        RuleFor(x => x.PropertyTypeId).NotEmpty();

        RuleFor(x => x.HeatTreatmentId)
            .NotEqual(Guid.Empty)
            .When(x => x.HeatTreatmentId.HasValue);

        MechanicalPropertyRules.AddCommonRules(this,
            x => x.ValueMin, x => x.ValueMax, x => x.ValueExact,
            x => x.Condition, x => x.Source, x => x.Notes,
            requireValue: true);
    }
}
