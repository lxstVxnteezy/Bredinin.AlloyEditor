using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507171639)]
    public class InitTableRefreshTokens_2025_07_17_1639: ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
        CREATE TABLE refresh_tokens (
            id          UUID PRIMARY KEY,
            user_id     UUID NOT NULL,
            token       TEXT NOT NULL UNIQUE,
            is_used      BOOL,
            is_revoked   BOOL,
            expires     TIMESTAMP NOT NULL,

            CONSTRAINT fk_refresh_tokens_user
                FOREIGN KEY (user_id) 
                REFERENCES users(id) 
                ON DELETE CASCADE
        );
    ");
        }
    }
}