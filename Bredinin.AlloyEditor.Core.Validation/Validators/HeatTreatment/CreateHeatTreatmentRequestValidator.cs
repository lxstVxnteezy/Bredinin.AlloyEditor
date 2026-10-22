using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.HeatTreatment;

public class CreateHeatTreatmentRequestValidator : AbstractValidator<CreateHeatTreatmentRequest>
{
    public CreateHeatTreatmentRequestValidator()
    {
        RuleFor(x => x.HeatTreatmentTypeId).NotEmpty();

        HeatTreatmentRules.AddCommonRules(this,
            x => x.TemperatureMin, x => x.TemperatureMax, x => x.TemperatureExact,
            x => x.HoldingTimeMin, x => x.HoldingTimeMax, x => x.HoldingTimeExact,
            x => x.CoolingMedium, x => x.Description, x => x.StepOrder,
            requireBoundsTogether: true);
    }
}
