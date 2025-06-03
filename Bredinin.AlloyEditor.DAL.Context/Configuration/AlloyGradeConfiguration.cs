using Bredinin.MyPetProject.Domain.Alloys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.MyPetProject.DAL.Configuration
{
    internal class AlloyGradeConfiguration: IEntityTypeConfiguration<AlloyGrade>
    {
        public void Configure(EntityTypeBuilder<AlloyGrade> builder)
        {
            builder.ToTable("alloy_grades");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.AlloySystemId).HasColumnName("alloy_system_id");
        }
    }
}
