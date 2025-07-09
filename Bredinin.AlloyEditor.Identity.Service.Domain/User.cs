using Bredinin.AlloyEditor.Identity.Service.Domain.Base;

namespace Bredinin.AlloyEditor.Identity.Service.Domain
{
    public class User: BaseEntity
    {
        public string Login { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public int Age { get; set; }
        public string Hash { get; set; } = null!;

        public virtual ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}
