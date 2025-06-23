using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202523062130)]

    public class AddFieldToTableUsers_2025_06_23_2130 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter
                .Table("users")
                .AddColumn("login")
                .AsString()
                .NotNullable();
        }
    }
}
