using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507072359)]
    public class InitTableUserRoles_2025_07_07_2359 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("user_roles")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("role_id").AsGuid().NotNullable();

            Create.Index("IX_user_roles_user_id_role_id")
                .OnTable("user_roles")
                .OnColumn("user_id").Ascending()
                .OnColumn("role_id").Ascending()
                .WithOptions().Unique();

            Create.ForeignKey("FK_user_roles_users")
                .FromTable("user_roles").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.ForeignKey("FK_user_roles_roles")
                .FromTable("user_roles").ForeignColumn("role_id")
                .ToTable("roles").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);
        }
    }
}
