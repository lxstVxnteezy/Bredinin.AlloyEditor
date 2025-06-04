using FluentMigrator;

namespace Bredinin.MyPetProject.DAL.Migration.Migrations
{
    [Migration(202504062208)]
    public class AddFieldToTableAlloySystems_2025_04_06_2208:ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("alloy_systems").AddColumn("base_element_id").AsGuid().NotNullable().Unique();

            Create.ForeignKey("fk_alloy_systems")
                .FromTable("alloy_systems").ForeignColumn("base_element_id")
                .ToTable("dict_chemical_elements").PrimaryColumn("id");
        }
    }
}
