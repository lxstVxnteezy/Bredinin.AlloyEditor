namespace Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions
{
    public record ChemicalCompositionsResponse(Guid Id,
        decimal? MinValue,
        decimal? MaxValue,
        decimal? ExactValue,
        Guid ChemicalElementId);
}