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

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Description).HasColumnName("description");
        }
    }
}
