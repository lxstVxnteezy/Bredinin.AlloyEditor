using FluentMigrator;

namespace Bredinin.MyPetProject.DAL.Migration.Mirgations
{
    [Migration(202513051848)]
    public class InitialMigration_13_05_2025_1848 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(
                @"CREATE TABLE users (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                first_name VARCHAR(100) NOT NULL,
                last_name VARCHAR(100) NOT NULL,
                second_name VARCHAR(100) NOT NULL,
                age INT NOT NULL)");
        }
    }
}
