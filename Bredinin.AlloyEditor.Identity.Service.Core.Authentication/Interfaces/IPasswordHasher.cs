namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces
{ 
    public interface IPasswordHasher
    {
        string Generate(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
