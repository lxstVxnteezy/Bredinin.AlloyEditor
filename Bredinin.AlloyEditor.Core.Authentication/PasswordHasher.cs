using Bredinin.AlloyEditor.Core.Authentication.Interfaces;

namespace Bredinin.AlloyEditor.Core.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Generate(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        public bool VerifyPassword(string password, string passwordHash) => 
            BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}
