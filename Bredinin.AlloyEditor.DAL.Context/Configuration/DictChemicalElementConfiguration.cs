using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.AlloyEditor.DAL.Configuration
{
    internal class DictChemicalElementConfiguration : IEntityTypeConfiguration<DictChemicalElement>
    {
        public void Configure(EntityTypeBuilder<DictChemicalElement> builder)
        {
            builder.ToTable("dict_chemical_elements");

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.IsBaseForAlloySystem).HasColumnName("is_base_for_alloy_system");
            builder.Property(x => x.Symbol).HasColumnName("symbol");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.AtomicNumber).HasColumnName("atomic_number");
            builder.Property(x => x.AtomicWeight).HasColumnName("atomic_weight");
            builder.Property(x => x.Group).HasColumnName("group");
            builder.Property(x => x.Period).HasColumnName("period");
            builder.Property(x => x.Density).HasColumnName("density");
        }
    }
}