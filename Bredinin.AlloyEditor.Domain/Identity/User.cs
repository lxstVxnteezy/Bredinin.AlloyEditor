namespace Bredinin.AlloyEditor.Domain.Identity;

public class User : BaseEntity
{
    public string Login { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string SecondName { get; set; } = null!;
    public int Age { get; set; }
    public string Hash { get; set; } = null!;

    public ICollection<RoleUser> RoleUsers { get; set; } 
        = new List<RoleUser>();
}