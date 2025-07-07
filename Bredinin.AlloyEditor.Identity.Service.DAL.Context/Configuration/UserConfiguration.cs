using Bredinin.AlloyEditor.Identity.Service.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.DAL.Context.Configuration
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Login).HasColumnName("login");
            builder.Property(x => x.FirstName).HasColumnName("first_name");
            builder.Property(x => x.LastName).HasColumnName("last_name");
            builder.Property(x => x.SecondName).HasColumnName("second_name");
            builder.Property(x => x.Age).HasColumnName("age");
            builder.Property(x => x.Hash).HasColumnName("hash");
        }
    }
}
