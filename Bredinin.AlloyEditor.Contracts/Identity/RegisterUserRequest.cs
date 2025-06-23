namespace Bredinin.AlloyEditor.Contracts.Identity
{
    public record RegisterUserRequest(
        string Login, 
        string FirstName, 
        string LastName, 
        string SecondName, 
        int Age, 
        string Password);
}