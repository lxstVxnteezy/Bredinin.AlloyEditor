using Bredinin.MyPetProject.DAL.Migration.Parsers;
using FluentMigrator;

namespace Bredinin.MyPetProject.DAL.Migration.Mirgations
{
    [Migration(202504060146)]
    public class AddDataForChemicalDict_04_06_2025_0146 : ForwardOnlyMigration
    {
        public override void Up()
        {
            var elements = ChemicalElementExcelParser.ParseElements();

            foreach (var element in elements)
            {
                Insert.IntoTable("dict_chemical_elements")
                    .Row(new
                    {
                        id = element.Id,
                        name = element.Name,
                        symbol = element.Symbol,
                        description = element.Description,
                        is_base_for_alloy_system = element.IsBaseForAlloySystem,
                        atomic_number = element.AtomicNumber,
                        atomic_weight = element.AtomicWeight,
                        group = element.Group,
                        period = element.Period,
                        density = element.Density
                    });
            }
        }
    }
}
