using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507072357)]
    public class InitTableUser_07_07_2025_2357 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
            CREATE TABLE users (
                id         UUID PRIMARY KEY,
                login      VARCHAR(60) NOT NULL UNIQUE,
                first_name VARCHAR(100) NOT NULL,
                last_name  VARCHAR(100) NOT NULL,
                second_name VARCHAR(100) NULL,
                age        INT NOT NULL,
                hash       VARCHAR NOT NULL
            );
        ");
        }
    }
}