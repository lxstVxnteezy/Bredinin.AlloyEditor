using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class AlloyChemicalCompositionConfiguration : IEntityTypeConfiguration<AlloyChemicalCompositions>
    {
        public void Configure(EntityTypeBuilder<AlloyChemicalCompositions> builder)
        {
            builder.ToTable("alloy_chemical_compositions");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.AlloyGradeId).HasColumnName("alloy_grade_id");
            builder.Property(x => x.ChemicalElementId).HasColumnName("chemical_element_id");
            builder.Property(x => x.ExactValue).HasColumnName("exact_value");
            builder.Property(x => x.MaxValue).HasColumnName("max_value");
            builder.Property(x => x.MinValue).HasColumnName("min_value");

            builder.HasIndex(x => x.AlloyGradeId);
            builder.HasIndex(x => x.ChemicalElementId);

            builder.HasOne(x => x.AlloyGrade)
                .WithMany(x => x.ChemicalCompositions)
                .HasForeignKey(x => x.AlloyGradeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChemicalElement)
                .WithMany()
                .HasForeignKey(x => x.ChemicalElementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}