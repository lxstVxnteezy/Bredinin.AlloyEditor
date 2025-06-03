using Bredinin.MyPetProject.Domain.Dictionaries;

namespace Bredinin.MyPetProject.Domain.Alloys
{
    public class AlloyChemicalCompositions : BaseEntity
    {
        public decimal? MinValue { get; set; }  // Нижняя граница (может быть null если не определена)
        public decimal? MaxValue { get; set; }  // Верхняя граница (может быть null если не определена)
        public decimal? ExactValue { get; set; }  // Точное значение (если требуется)

        public Guid AlloyGradeId { get; set; }
        public Guid ChemicalElementId { get; set; }

        public AlloyGrade AlloyGrade { get; set; } = null!;
        public DictChemicalElement ChemicalElement { get; set; } = null!;
    }
}
