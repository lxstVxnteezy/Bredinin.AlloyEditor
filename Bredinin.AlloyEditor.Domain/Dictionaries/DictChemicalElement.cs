namespace Bredinin.AlloyEditor.Domain.Dictionaries
{ 
    public class DictChemicalElement : BaseEntity
    { 
        public string Name { get; set; } = null!;
        public string Symbol = null!;
        public string? Description { get; set; }
        public bool IsBaseForAlloySystem { get; set; } 
        public int AtomicNumber { get; set; }
        public decimal AtomicWeight { get; set; }
        public int Group { get; set; }
        public int Period { get; set; }
        public decimal Density { get; set; }
    }
}
