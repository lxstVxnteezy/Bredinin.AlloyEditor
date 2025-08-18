using FluentMigrator;

namespace Bredinin.AlloyEditor.Identity.Service.Migration.Migrations
{
    [Migration(202508142116)]
    public class InsertValuesInTables_2025_08_14_2116 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
                INSERT INTO roles (id, name, description)
                VALUES
                    ('1798a758-829b-4839-9180-0a205545a450', 'admin', 'admin'),
                    ('7a593027-99d0-4a47-91b1-5c73049cecc1', 'user', 'user');
            ");

            Execute.Sql(@"
                INSERT INTO users (id, login, first_name, last_name, second_name, age, hash)
                VALUES (
                    '0198a9d0-2f1b-7d41-99d6-33a2ef280914',
                    'admin',
                    'admin',
                    'admin',
                    'admin',
                    22,
                    '$2a$11$SHGUnSbdFb0DUEb.zqqIX.c8MPM8yMjl/NMrO23l3naDlAnM5lXHu'
                );
            ");

            Execute.Sql(@"
                INSERT INTO user_roles (id, user_id, role_id)
                VALUES (
                        '0198a9d9-930e-73d4-b877-5301e1a4bc46',
                        '0198a9d0-2f1b-7d41-99d6-33a2ef280914',
                        '1798a758-829b-4839-9180-0a205545a450'
                );
            ");
        }
    }
}
