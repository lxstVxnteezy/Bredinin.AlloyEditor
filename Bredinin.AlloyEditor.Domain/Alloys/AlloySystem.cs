using Bredinin.AlloyEditor.Domain.Dictionaries;

namespace Bredinin.AlloyEditor.Domain.Alloys
{
    public class AlloySystem : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }

        public DictChemicalElement BaseElement { get; set; } = null!;
        public Guid BaseElementId { get; set; }

        public ICollection<AlloyGrade> Grades { get; set; } = new List<AlloyGrade>();
    }
}
