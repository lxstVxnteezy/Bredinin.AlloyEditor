namespace Bredinin.AlloyEditor.Domain.Identity
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
