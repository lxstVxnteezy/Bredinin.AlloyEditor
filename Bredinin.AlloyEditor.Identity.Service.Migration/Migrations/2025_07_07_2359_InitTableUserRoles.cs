using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202507072359)]
    public class InitTableUserRoles_2025_07_07_2359 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
        CREATE TABLE user_roles (
            id       UUID PRIMARY KEY,
            user_id  UUID NOT NULL,
            role_id  UUID NOT NULL,

            CONSTRAINT uq_user_roles_pair 
                UNIQUE (user_id, role_id),

            CONSTRAINT fk_user_roles_user
                FOREIGN KEY (user_id) 
                REFERENCES users (id) 
                ON DELETE CASCADE,
                
            CONSTRAINT fk_user_roles_role
                FOREIGN KEY (role_id) 
                REFERENCES roles (id) 
                ON DELETE CASCADE
        );
    ");
        }
    }
}
