using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507072358)]
    public class InitTableRole_2025_07_07_2358 : ForwardOnlyMigration
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
