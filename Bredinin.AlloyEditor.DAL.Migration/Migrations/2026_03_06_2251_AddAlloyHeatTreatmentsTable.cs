using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202603062251)]
    public class AddAlloyHeatTreatmentsTable_2026_03_06_2251 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("alloy_heat_treatments")
                           .WithColumn("id").AsGuid().PrimaryKey()
                           .WithColumn("alloy_grade_id").AsGuid().NotNullable()
                           .WithColumn("heat_treatment_type_id").AsGuid().NotNullable()
                           .WithColumn("temperature_min").AsInt32().Nullable()
                           .WithColumn("temperature_max").AsInt32().Nullable()
                           .WithColumn("temperature_exact").AsInt32().Nullable()
                           .WithColumn("holding_time_min").AsInt32().Nullable()
                           .WithColumn("holding_time_max").AsInt32().Nullable()
                           .WithColumn("holding_time_exact").AsInt32().Nullable()
                           .WithColumn("cooling_medium").AsString(100).Nullable()
                           .WithColumn("description").AsString(1000).Nullable()
                           .WithColumn("step_order").AsInt32().Nullable()
                           .WithColumn("is_default").AsBoolean().WithDefaultValue(false);

            // Внешние ключи
            Create.ForeignKey("fk_heat_treatments_alloy_grade")
                .FromTable("alloy_heat_treatments").ForeignColumn("alloy_grade_id")
                .ToTable("alloy_grades").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.ForeignKey("fk_heat_treatments_type")
                .FromTable("alloy_heat_treatments").ForeignColumn("heat_treatment_type_id")
                .ToTable("dict_types_of_heat_treatments").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.None); // Restrict

            // Индексы
            Create.Index("ix_heat_treatments_alloy_grade_id")
                .OnTable("alloy_heat_treatments").OnColumn("alloy_grade_id");

            Create.Index("ix_heat_treatments_type_id")
                .OnTable("alloy_heat_treatments").OnColumn("heat_treatment_type_id");

            Create.Index("ix_heat_treatments_composite")
                .OnTable("alloy_heat_treatments")
                .OnColumn("alloy_grade_id").Ascending()
                .OnColumn("heat_treatment_type_id").Ascending()
                .OnColumn("step_order").Ascending()
                .WithOptions().Unique();
        }
    }
}
