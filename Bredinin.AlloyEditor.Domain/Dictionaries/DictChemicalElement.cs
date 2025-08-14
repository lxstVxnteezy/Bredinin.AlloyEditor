namespace Bredinin.AlloyEditor.Domain.Dictionaries
{ 
    public class DictChemicalElement : BaseEntity
    {
        public required string Name { get; set; }
      
        public required string Symbol { get; set; }
       
        public string? Description { get; set; }
       
        public bool IsBaseForAlloySystem { get; set; } 
       
        public int AtomicNumber { get; set; }
       
        public decimal AtomicWeight { get; set; }
       
        public int Group { get; set; }
       
        public int Period { get; set; }
       
        public decimal Density { get; set; }
    }
}
