using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class DictMechanicalPropertyTypeConfiguration : IEntityTypeConfiguration<DictMechanicalPropertyType>
    {
        public void Configure(EntityTypeBuilder<DictMechanicalPropertyType> builder)
        {
            builder.ToTable("dict_mechanical_property_types");

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Unit)
                .HasColumnName("unit")
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Symbol)
                .HasColumnName("symbol")
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            builder.Property(x => x.ValueType)
                .HasColumnName("value_type")
                .IsRequired()
                .HasDefaultValue(PropertyValueType.Range);

            builder.Property(x => x.MinPossible)
                .HasColumnName("min_possible")
                .HasPrecision(18, 4);

            builder.Property(x => x.MaxPossible)
                .HasColumnName("max_possible")
                .HasPrecision(18, 4);

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("ix_dict_mech_prop_name_unique");

            builder.HasIndex(x => x.Symbol)
                .HasDatabaseName("ix_dict_mech_prop_symbol");
        }
    }
}