using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.MechanicalProperty;

public class UpdateMechanicalPropertyRequestValidator : AbstractValidator<UpdateMechanicalPropertyRequest>
{
    public UpdateMechanicalPropertyRequestValidator()
    {
        RuleFor(x => x.PropertyTypeId)
            .NotEqual(Guid.Empty)
            .When(x => x.PropertyTypeId.HasValue);

        // Частичное обновление: Min и Max могут приходить по отдельности,
        // итоговая согласованность проверяется в хендлере по значениям из БД
        MechanicalPropertyRules.AddCommonRules(this,
            x => x.ValueMin, x => x.ValueMax, x => x.ValueExact,
            x => x.Condition, x => x.Source, x => x.Notes,
            requireValue: false);
    }
}
