using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class DictTypeOfHeatTreatmentConfiguration : IEntityTypeConfiguration<DictTypeOfHeatTreatment>
    {
        public void Configure(EntityTypeBuilder<DictTypeOfHeatTreatment> builder)
        {
            builder.ToTable("dict_types_of_heat_treatments");
            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(50);

            builder.Property(x => x.DefaultTemperatureMin)
                .HasColumnName("default_temperature_min");

            builder.Property(x => x.DefaultTemperatureMax)
                .HasColumnName("default_temperature_max");

            builder.Property(x => x.DefaultCoolingMedium)
                .HasColumnName("default_cooling_medium")
                .HasMaxLength(100);

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("ix_dict_heat_treatment_name");

            builder.HasIndex(x => x.Code)
                .HasDatabaseName("ix_dict_heat_treatment_code");
        }
    }
}
