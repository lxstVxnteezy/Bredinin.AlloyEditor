namespace Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions
{
    public record ChemicalCompositionsDto(Guid Id,
        decimal? MinValue,
        decimal? MaxValue,
        decimal? ExactValue,
        Guid ChemicalElementId);
}