using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class AlloyHeatTreatmentConfiguration : IEntityTypeConfiguration<AlloyHeatTreatment>
    {
        public void Configure(EntityTypeBuilder<AlloyHeatTreatment> builder)
        {
            builder.ToTable("alloy_heat_treatments");

            // Primary Key
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            // Поля
            builder.Property(x => x.AlloyGradeId)
                .HasColumnName("alloy_grade_id")
                .IsRequired();

            builder.Property(x => x.HeatTreatmentTypeId)
                .HasColumnName("heat_treatment_type_id")
                .IsRequired();

            builder.Property(x => x.TemperatureMin)
                .HasColumnName("temperature_min");

            builder.Property(x => x.TemperatureMax)
                .HasColumnName("temperature_max");

            builder.Property(x => x.TemperatureExact)
                .HasColumnName("temperature_exact");

            builder.Property(x => x.HoldingTimeMin)
                .HasColumnName("holding_time_min");

            builder.Property(x => x.HoldingTimeMax)
                .HasColumnName("holding_time_max");

            builder.Property(x => x.HoldingTimeExact)
                .HasColumnName("holding_time_exact");

            builder.Property(x => x.CoolingMedium)
                .HasColumnName("cooling_medium")
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.StepOrder)
                .HasColumnName("step_order");

            builder.Property(x => x.IsDefault)
                .HasColumnName("is_default")
                .HasDefaultValue(false);

            // Индексы
            builder.HasIndex(x => x.AlloyGradeId)
                .HasDatabaseName("ix_heat_treatments_alloy_grade_id");

            builder.HasIndex(x => x.HeatTreatmentTypeId)
                .HasDatabaseName("ix_heat_treatments_type_id");

            builder.HasIndex(x => new { x.AlloyGradeId, x.HeatTreatmentTypeId, x.StepOrder })
                .HasDatabaseName("ix_heat_treatments_composite")
                .IsUnique(); // Чтобы не было дублей одного типа для сплава с тем же порядком

            // Связи
            builder.HasOne(x => x.HeatTreatmentType)
                .WithMany()  // 👈 БЕЗ навигационного свойства!
                .HasForeignKey(x => x.HeatTreatmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}