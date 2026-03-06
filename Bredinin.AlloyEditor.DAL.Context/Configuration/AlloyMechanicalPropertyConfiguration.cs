using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class AlloyMechanicalPropertyConfiguration : IEntityTypeConfiguration<AlloyMechanicalProperty>
    {
        public void Configure(EntityTypeBuilder<AlloyMechanicalProperty> builder)
        {
            builder.ToTable("alloy_mechanical_properties");

            // Primary Key
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            // Поля
            builder.Property(x => x.AlloyGradeId)
                .HasColumnName("alloy_grade_id")
                .IsRequired();

            builder.Property(x => x.PropertyTypeId)
                .HasColumnName("property_type_id")
                .IsRequired();

            builder.Property(x => x.HeatTreatmentId)
                .HasColumnName("heat_treatment_id");

            builder.Property(x => x.ValueMin)
                .HasColumnName("value_min")
                .HasPrecision(18, 4);

            builder.Property(x => x.ValueMax)
                .HasColumnName("value_max")
                .HasPrecision(18, 4);

            builder.Property(x => x.ValueExact)
                .HasColumnName("value_exact")
                .HasPrecision(18, 4);

            builder.Property(x => x.Condition)
                .HasColumnName("condition")
                .HasMaxLength(500);

            builder.Property(x => x.Source)
                .HasColumnName("source")
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            // Индексы
            builder.HasIndex(x => x.AlloyGradeId)
                .HasDatabaseName("ix_mech_props_alloy_grade_id");

            builder.HasIndex(x => x.PropertyTypeId)
                .HasDatabaseName("ix_mech_props_type_id");

            builder.HasIndex(x => x.HeatTreatmentId)
                .HasDatabaseName("ix_mech_props_heat_treatment_id");

            // Уникальность: один сплав + один тип свойства + один режим ТО
            builder.HasIndex(x => new { x.AlloyGradeId, x.PropertyTypeId, x.HeatTreatmentId })
                .HasDatabaseName("ix_mech_props_unique")
                .IsUnique();

            // Связи
            builder.HasOne(x => x.AlloyGrade)
                .WithMany(x => x.MechanicalProperties)
                .HasForeignKey(x => x.AlloyGradeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PropertyType)
                .WithMany()  // Без обратной коллекции
                .HasForeignKey(x => x.PropertyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HeatTreatment)
                .WithMany(x => x.MechanicalProperties)
                .HasForeignKey(x => x.HeatTreatmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}