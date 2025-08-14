using Bredinin.AlloyEditor.Identity.Service.Domain.Base;

namespace Bredinin.AlloyEditor.Identity.Service.Domain
{
    public class Role : BaseEntity
    {
        public required string Name { get; set; }

        public required string Description { get; set; }
    }
}
