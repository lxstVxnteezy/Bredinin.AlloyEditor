using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202509061256)]
    public class InitTableDictTypeOfHeatTreatment_2025_06_09_1250 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("dict_types_of_heat_treatments")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString().NotNullable()
                .WithColumn("descrpition").AsString().NotNullable();
        }
    }
}
