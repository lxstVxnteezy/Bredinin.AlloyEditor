using Bredinin.AlloyEditor.Contracts.Identity;
using Bredinin.AlloyEditor.Core.Authentication;
using Bredinin.AlloyEditor.Core.Authentication.Interfaces;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Identity
{
    public interface ILoginUserHandler : IHandler
    {
        Task<string> Handle(LoginUserRequest request, CancellationToken ctn);
    }
    internal class LoginUserHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher) : ILoginUserHandler
    {
        public async Task<string> Handle(LoginUserRequest request, CancellationToken ctn)
        {
            var user = await userRepository.Query
                .SingleOrDefaultAsync(x => x.Login == request.Login, ctn);

            if (user == null)
                throw new BusinessException("User not found");

            var result = passwordHasher.VerifyPassword(request.Password, user.Hash);

            if (result == false)
                throw new BusinessException("Failed to login");

            return JwtProvider.GenerateToken(user);
        }
    }
}
