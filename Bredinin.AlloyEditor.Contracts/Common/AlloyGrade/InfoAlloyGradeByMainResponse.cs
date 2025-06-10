using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;

namespace Bredinin.AlloyEditor.Contracts.Common.AlloyGrade
{
    public record InfoAlloyGradeByMainResponse(
        Guid Id,
        string Name,
        string? Description,
        ChemicalCompositionsResponse[] ChemicalCompositions);
}
