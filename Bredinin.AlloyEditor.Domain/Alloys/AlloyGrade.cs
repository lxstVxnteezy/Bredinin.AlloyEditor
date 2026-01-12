namespace Bredinin.AlloyEditor.Domain.Alloys
{
    public class AlloyGrade : BaseEntity
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public AlloySystem AlloySystem { get; set; } = null!;
        public Guid AlloySystemId { get; set; }

        public virtual ICollection<AlloyChemicalCompositions> ChemicalCompositions { get; set; } =
            new List<AlloyChemicalCompositions>();
        
        public virtual ICollection<AlloyHeatTreatment> HeatTreatments { get; set; } =
            new List<AlloyHeatTreatment>();

    }
}
