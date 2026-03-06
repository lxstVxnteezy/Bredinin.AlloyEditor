using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202603062255)]

    public class AddAlloyMechanicalPropertiesTable_2026_03_06_2255 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("alloy_mechanical_properties")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("alloy_grade_id").AsGuid().NotNullable()
                .WithColumn("property_type_id").AsGuid().NotNullable()
                .WithColumn("heat_treatment_id").AsGuid().Nullable()
                .WithColumn("value_min").AsDecimal(18, 4).Nullable()
                .WithColumn("value_max").AsDecimal(18, 4).Nullable()
                .WithColumn("value_exact").AsDecimal(18, 4).Nullable()
                .WithColumn("condition").AsString(500).Nullable()
                .WithColumn("source").AsString(500).Nullable()
                .WithColumn("notes").AsString(1000).Nullable();

            // Внешние ключи
            Create.ForeignKey("fk_mech_props_alloy_grade")
                .FromTable("alloy_mechanical_properties").ForeignColumn("alloy_grade_id")
                .ToTable("alloy_grades").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.ForeignKey("fk_mech_props_property_type")
                .FromTable("alloy_mechanical_properties").ForeignColumn("property_type_id")
                .ToTable("dict_mechanical_property_types").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.None); // Restrict — нельзя удалить тип, если он используется

            Create.ForeignKey("fk_mech_props_heat_treatment")
                .FromTable("alloy_mechanical_properties").ForeignColumn("heat_treatment_id")
                .ToTable("alloy_heat_treatments").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade); // При удалении режима ТО удаляются и его свойства

            // Индексы для ускорения запросов
            Create.Index("ix_mech_props_alloy_grade_id")
                .OnTable("alloy_mechanical_properties")
                .OnColumn("alloy_grade_id");

            Create.Index("ix_mech_props_property_type_id")
                .OnTable("alloy_mechanical_properties")
                .OnColumn("property_type_id");

            Create.Index("ix_mech_props_heat_treatment_id")
                .OnTable("alloy_mechanical_properties")
                .OnColumn("heat_treatment_id");

            // Составной индекс для поиска по сплаву + типу свойства + режиму ТО
            Create.Index("ix_mech_props_alloy_type_treatment")
                .OnTable("alloy_mechanical_properties")
                .OnColumn("alloy_grade_id").Ascending()
                .OnColumn("property_type_id").Ascending()
                .OnColumn("heat_treatment_id").Ascending();

            // Уникальное ограничение — один сплав + один тип свойства + один режим ТО
            Create.UniqueConstraint("uq_alloy_mechanical_property")
                .OnTable("alloy_mechanical_properties")
                .Columns("alloy_grade_id", "property_type_id", "heat_treatment_id");
        }
    }
}
