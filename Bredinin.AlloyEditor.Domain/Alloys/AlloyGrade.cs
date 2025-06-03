namespace Bredinin.MyPetProject.Domain.Alloys
{
    public class AlloyGrade : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public Guid AlloySystemId { get; set; }
        public AlloySystem AlloySystem { get; set; } = null!;


        public ICollection<AlloyChemicalCompositions> ChemicalCompositions { get; set; } =
            new List<AlloyChemicalCompositions>();
    }
}
