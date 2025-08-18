using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Bredinin.AlloyEditor.Identity.Service.Domain;

namespace Bredinin.AlloyEditor.Identity.Service.DAL.Context.Configuration
{
    internal class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.Name).HasColumnName("name");

            builder.Property(x => x.Description).HasColumnName("description");
        }
    }
}
