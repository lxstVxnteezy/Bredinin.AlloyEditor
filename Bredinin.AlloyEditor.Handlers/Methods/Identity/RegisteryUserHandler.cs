using Bredinin.AlloyEditor.Contracts.Identity;
using Bredinin.AlloyEditor.Core.Authentication.Interfaces;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Domain.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Identity
{
    public interface IRegisterUserHandler : IHandler
    {
        public Task<ActionResult> Handle(RegisterUserRequest request, CancellationToken ctn);
    }
    internal class RegisterUserHandler(IRepository<User> usersRepository, IPasswordHasher hasher) : IRegisterUserHandler
    {
        public async Task<ActionResult> Handle(RegisterUserRequest request, CancellationToken ctn)
        {
            var foundUser = await usersRepository.Query
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Login == request.Login, ctn)
                .ConfigureAwait(false);

            if (foundUser != null)
                throw new BusinessException("already exist user");

            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                Login = request.Login,
                FirstName = request.FirstName,
                LastName = request.LastName,
                SecondName = request.SecondName,
                Age = request.Age,
                Hash = hasher.Generate(request.Password),
            };

            usersRepository.Add(newUser);
            await usersRepository.SaveChanges(ctn);

            return new StatusCodeResult(204);
        }
    }
}
