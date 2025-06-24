using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class RoleUser : IEntityTypeConfiguration<Domain.Identity.RoleUser>
    {
        public void Configure(EntityTypeBuilder<Domain.Identity.RoleUser> builder)
        {
            builder.ToTable("user_roles");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.RoleId).HasColumnName("role_id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
        }
    }
}
