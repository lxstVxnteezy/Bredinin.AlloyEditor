using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507072357)]
    public class InitTableUser_07_07_2025_2357 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("users")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("login").AsString().NotNullable()
                .WithColumn("first_name").AsString().Nullable()
                .WithColumn("last_name").AsString().Nullable()
                .WithColumn("second_name").AsString().Nullable()
                .WithColumn("age").AsInt32().Nullable()
                .WithColumn("hash").AsString().Nullable();
        }
    }
}