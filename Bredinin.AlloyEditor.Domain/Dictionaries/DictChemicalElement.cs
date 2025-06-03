namespace Bredinin.MyPetProject.Domain.Dictionaries
{ 
    public class DictChemicalElement : BaseEntity
    { 
        public string Name { get; set; } = null!;
        public string Symbol = null!;
        public string? Description { get; set; }
        public bool IsBaseForAlloySystem { get; set; } // Является ли элемент основой для системы сплавов
    }
}
