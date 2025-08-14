using Bredinin.AlloyEditor.Domain.Dictionaries;

namespace Bredinin.AlloyEditor.Domain.Alloys
{
    public class AlloyChemicalCompositions : BaseEntity
    {
        public decimal? MinValue { get; set; } 
        public decimal? MaxValue { get; set; }  
        public decimal? ExactValue { get; set; }  

        public Guid AlloyGradeId { get; set; }
        public AlloyGrade AlloyGrade { get; set; } = null!;

        public Guid ChemicalElementId { get; set; }
        public DictChemicalElement ChemicalElement { get; set; } = null!;
    }
}
