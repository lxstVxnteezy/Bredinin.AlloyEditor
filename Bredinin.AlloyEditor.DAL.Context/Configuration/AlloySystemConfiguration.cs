using Bredinin.MyPetProject.Domain.Alloys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.MyPetProject.DAL.Configuration
{
    internal class AlloySystemConfiguration : IEntityTypeConfiguration<AlloySystem>
    {
        public void Configure(EntityTypeBuilder<AlloySystem> builder)
        {
            builder.ToTable("alloy_systems");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.BaseElementId).HasColumnName("base_element_id");
            builder.Property(x => x.Description).HasColumnName("description");
        }
    }
}
