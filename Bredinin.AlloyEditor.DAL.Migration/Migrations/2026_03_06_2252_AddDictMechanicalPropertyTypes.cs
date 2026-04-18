using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202603062252)]
    public class AddDictMechanicalPropertyTypes_2026_03_06_2252 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("dict_mechanical_property_types")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("unit").AsString(50).NotNullable()
                .WithColumn("symbol").AsString(50).Nullable()
                .WithColumn("description").AsString(1000).Nullable()
                .WithColumn("value_type").AsInt32().NotNullable().WithDefaultValue(0) // 0-Range, 1-Exact
                .WithColumn("min_possible").AsDecimal(18, 4).Nullable()
                .WithColumn("max_possible").AsDecimal(18, 4).Nullable();

            // Уникальный индекс на названии
            Create.Index("ix_dict_mech_prop_name_unique")
                .OnTable("dict_mechanical_property_types")
                .OnColumn("name").Unique();

            // Индекс на символе (если нужно искать по обозначению)
            Create.Index("ix_dict_mech_prop_symbol")
                .OnTable("dict_mechanical_property_types")
                .OnColumn("symbol");
        }
    }
}
