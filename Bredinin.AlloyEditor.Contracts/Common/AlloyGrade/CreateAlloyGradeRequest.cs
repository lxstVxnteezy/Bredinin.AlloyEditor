using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;

namespace Bredinin.AlloyEditor.Contracts.Common.AlloyGrade
{
    public record CreateAlloyGradeRequest(
        string Name, 
        string? Description, 
        Guid AlloySystemId, 
        CreateChemicalCompositionRequest[] ChemicalCompositions,
        CreateHeatTreatmentRequest[]? HeatTreatments,
        CreateMechanicalPropertyRequest[]? DefaultMechanicalProperties);
    public record CreateChemicalCompositionRequest(decimal? MinValue,
        decimal? MaxValue,
        decimal? ExactValue,
        Guid ChemicalElementId);
}
