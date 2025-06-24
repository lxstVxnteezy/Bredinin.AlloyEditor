using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202524060105)]
    public class InitTableRoles_2025_06_24_0105 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("roles")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString().NotNullable()
                .WithColumn("description").AsString().Nullable();
        }
    }
}
