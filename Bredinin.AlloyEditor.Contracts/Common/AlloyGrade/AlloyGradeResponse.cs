using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;

namespace Bredinin.AlloyEditor.Contracts.Common.AlloyGrade
{
    public record AlloyGradeResponse(
        Guid Id,
        string Name,
        string? Description,
        Guid AlloySystemId,
        ChemicalCompositionsDto[] ChemicalCompositions,
        HeatTreatmentDto[] HeatTreatments,
        MechanicalPropertyDto[] MechanicalProperties);
}
