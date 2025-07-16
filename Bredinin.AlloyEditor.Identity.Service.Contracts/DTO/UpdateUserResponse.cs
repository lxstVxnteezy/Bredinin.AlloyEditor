namespace Bredinin.AlloyEditor.Identity.Service.Contracts.DTO
{
    public record UpdateUserResponse(
        string Login, 
        string FirstName,
        string LastName, 
        string SecondName,
        Guid[] RoleIds);

}
