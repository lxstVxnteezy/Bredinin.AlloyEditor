using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507171639)]
    public class InitTableRefreshTokens_2025_07_17_1639: ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("refresh_tokens").WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("user_id").AsGuid().ForeignKey()
                .WithColumn("token").AsString(int.MaxValue).NotNullable()
                .WithColumn("is_used").AsBoolean()
                .WithColumn("is_revoked").AsBoolean()
                .WithColumn("expires").AsDateTime().NotNullable();

            Create.Index("IX_refresh_tokens_token")
                .OnTable("refresh_tokens")
                .OnColumn("token").Unique();
        }
    }
}