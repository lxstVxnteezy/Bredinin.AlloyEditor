using Bredinin.MyPetProject.Domain.Dictionaries;

namespace Bredinin.MyPetProject.Domain.Alloys
{
    public class AlloyChemicalCompositions : BaseEntity
    {
        public decimal? MinValue { get; set; } 
        public decimal? MaxValue { get; set; }  
        public decimal? ExactValue { get; set; }  

        public Guid AlloyGradeId { get; set; }
        public Guid ChemicalElementId { get; set; }

        public virtual AlloyGrade AlloyGrade { get; set; } = null!;
        public virtual DictChemicalElement ChemicalElement { get; set; } = null!;
    }
}
