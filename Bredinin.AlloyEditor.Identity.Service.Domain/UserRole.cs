﻿using Bredinin.AlloyEditor.Identity.Service.Domain.Base;

namespace Bredinin.AlloyEditor.Identity.Service.Domain
{
    public class UserRole: BaseEntity
    {
        public virtual Role Role { get; set; } = null!;
        public Guid RoleId { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
