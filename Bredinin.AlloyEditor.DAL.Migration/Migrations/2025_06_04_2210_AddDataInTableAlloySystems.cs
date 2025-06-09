using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202504062210)]
    public class AddDataInTableAlloySystems_2025_06_04_2210 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
            INSERT INTO alloy_systems (id, name, base_element_id)
            SELECT 
                gen_random_uuid(),
                symbol || ' - Alloy',
                id
            FROM dict_chemical_elements
            WHERE is_base_for_alloy_system = true
        ");
        }
    }
}
