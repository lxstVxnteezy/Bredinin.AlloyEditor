﻿using Bredinin.AlloyEditor.Domain.Dictionaries;

namespace Bredinin.AlloyEditor.Domain.Alloys
{
    public class AlloySystem : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid BaseElementId { get; set; }  

        public virtual DictChemicalElement BaseElement { get; set; } = null!;
        public virtual ICollection<AlloyGrade> Grades { get; set; } = new List<AlloyGrade>();
    }
}
