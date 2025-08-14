using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507072358)]
    public class InitTableRole_2025_07_07_2358 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
            CREATE TABLE roles (
                id           UUID PRIMARY KEY,
                name         VARCHAR(60) NOT NULL UNIQUE,
                description  VARCHAR NOT NULL        
            );
        ");
        }
    }
}

 