namespace Bredinin.AlloyEditor.Domain.Alloys
{
    public class AlloyGrade : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public Guid AlloySystemId { get; set; }
        public virtual AlloySystem AlloySystem { get; set; } = null!;

        public virtual ICollection<AlloyChemicalCompositions> ChemicalCompositions { get; set; } =
            new List<AlloyChemicalCompositions>();
        
        public virtual ICollection<AlloyHeatTreatment> HeatTreatments { get; set; } =
            new List<AlloyHeatTreatment>(); 
    }
}
