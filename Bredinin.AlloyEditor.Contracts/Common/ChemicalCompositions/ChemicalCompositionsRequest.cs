namespace Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions
{
    public record ChemicalCompositionsRequest(decimal? MinValue,
        decimal? MaxValue,
        decimal? ExactValue,
        Guid ChemicalElementId);
}
