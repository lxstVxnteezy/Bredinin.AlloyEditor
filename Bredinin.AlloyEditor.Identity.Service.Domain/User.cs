using Bredinin.AlloyEditor.Identity.Service.Domain.Base;

namespace Bredinin.AlloyEditor.Identity.Service.Domain
{
    public class User: BaseEntity
    {
        public required string Login { get; set; } 

        public required string FirstName { get; set; } 

        public required string LastName { get; set; } 

        public required string SecondName { get; set; } 

        public int Age { get; set; }

        public required string Hash { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}
