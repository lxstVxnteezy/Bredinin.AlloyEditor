using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.HeatTreatment;

public class UpdateHeatTreatmentRequestValidator : AbstractValidator<UpdateHeatTreatmentRequest>
{
    public UpdateHeatTreatmentRequestValidator()
    {
        RuleFor(x => x.HeatTreatmentTypeId)
            .NotEqual(Guid.Empty)
            .When(x => x.HeatTreatmentTypeId.HasValue);

        HeatTreatmentRules.AddCommonRules(this,
            x => x.TemperatureMin, x => x.TemperatureMax, x => x.TemperatureExact,
            x => x.HoldingTimeMin, x => x.HoldingTimeMax, x => x.HoldingTimeExact,
            x => x.CoolingMedium, x => x.Description, x => x.StepOrder,
            requireBoundsTogether: false);
    }
}
