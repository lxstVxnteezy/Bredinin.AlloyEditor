﻿using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin
{
    public record ResetPasswordUserCommand : IRequest 
    {
        public Guid UserId { get; set; }
        public string Password { get; set; } = null!;
    }
}
