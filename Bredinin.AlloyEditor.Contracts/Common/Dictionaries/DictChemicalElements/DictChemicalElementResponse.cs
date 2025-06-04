namespace Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements
{
    public record DictChemicalElementResponse(Guid Id, 
        string Name,
        string Symbol, 
        string? Description,
        bool IsBaseForAlloySystem,
        int AtomicNumber,
        decimal AtomicWeight,
        int Group,
        int Period,
        decimal Density);
}
