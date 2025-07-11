﻿using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202513052009)]
    public class AddFieldToTablesUsers_13_05_2025_2009 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter
                .Table("users")
                .AddColumn("hash")
                .AsString()
                .NotNullable();
        }
    }
}
