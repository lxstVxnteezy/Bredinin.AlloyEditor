namespace Bredinin.AlloyEditor.Contracts.Common.AlloyGrade
{
    public record CreateAlloyGradeRequest(string Name, string? Description, Guid AlloySystemId, CreateChemicalCompositionRequest[] ChemicalCompositions);
    public record CreateChemicalCompositionRequest(decimal? MinValue,
        decimal? MaxValue,
        decimal? ExactValue,
        Guid ChemicalElementId);
}
