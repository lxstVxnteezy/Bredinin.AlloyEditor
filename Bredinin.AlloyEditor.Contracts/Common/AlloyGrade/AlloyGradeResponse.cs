using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;

namespace Bredinin.AlloyEditor.Contracts.Common.AlloyGrade
{
    public record AlloyGradeResponse(
        Guid Id,
        string Name,
        string? Description,
        Guid AlloySystemId,
        ChemicalCompositionsDto[] ChemicalCompositions);
}
