namespace Bredinin.MyPetProject.Domain;

public class User : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string SecondName { get; set; } = null!;
    public int Age { get; set; }
    public string Hash { get; set; } = null!;
}