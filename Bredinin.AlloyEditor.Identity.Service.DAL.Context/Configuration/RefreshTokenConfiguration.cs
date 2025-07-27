using Bredinin.AlloyEditor.Identity.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.Identity.Service.DAL.Context.Configuration
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.Token).HasColumnName("token");
            builder.Property(x => x.IsUsed).HasColumnName("is_used");
            builder.Property(x => x.IsRevoked).HasColumnName("is_revoked");
            builder.Property(x => x.Expires).HasColumnName("expires");
        }
    }
}