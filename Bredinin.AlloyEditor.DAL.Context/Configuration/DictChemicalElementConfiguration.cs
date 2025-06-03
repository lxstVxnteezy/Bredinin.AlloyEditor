using Bredinin.MyPetProject.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bredinin.MyPetProject.DAL.Configuration
{
    internal class DictChemicalElementConfiguration : IEntityTypeConfiguration<DictChemicalElement>
    {
        public void Configure(EntityTypeBuilder<DictChemicalElement> builder)
        {
            builder.ToTable("dictionaries_chemical_element");

            builder.Property(x => x.IsBaseForAlloySystem).HasColumnName("isBaseForAlloySystem");
            builder.Property(x => x.Symbol).HasColumnName("symbol");
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Description).HasColumnName("description");
        }
    }
}
