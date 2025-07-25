﻿using Bredinin.AlloyEditor.Identity.Service.Domain.Base;

namespace Bredinin.AlloyEditor.Identity.Service.Domain
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
