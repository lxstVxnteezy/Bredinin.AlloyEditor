namespace Bredinin.AlloyEditor.Domain.Identity
{
    public class RoleUser : BaseEntity
    {
        public Role Role { get; set; } = null!;
        public Guid RoleId { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
