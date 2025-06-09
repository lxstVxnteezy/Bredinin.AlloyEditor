using System.Data;
using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202503062149)]
    public class InitTables_AlloyGrade_AlloySystem_DictChemical_AlloyChemicalComposition_03_06_2025_2149 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("dict_chemical_elements")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("symbol").AsString(10).NotNullable()
                .WithColumn("description").AsString(1000).Nullable()
                .WithColumn("is_base_for_alloy_system").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Table("alloy_systems")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("description").AsString(1000).Nullable();

            Create.Table("alloy_grades")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("description").AsString(1000).Nullable()
                .WithColumn("alloy_system_id").AsGuid().NotNullable()
                    .ForeignKey("fk_alloy_grades_alloy_systems", "alloy_systems", "id");

            Create.Table("alloy_chemical_compositions")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("alloy_grade_id").AsGuid().NotNullable()
                    .ForeignKey("fk_compositions_alloy_grades", "alloy_grades", "id")
                     .OnDelete(Rule.Cascade)
                .WithColumn("chemical_element_id").AsGuid().NotNullable()
                    .ForeignKey("fk_compositions_chemical_elements", "dict_chemical_elements", "id")
                .WithColumn("min_value").AsDecimal().Nullable()
                .WithColumn("max_value").AsDecimal().Nullable()
                .WithColumn("exact_value").AsDecimal().Nullable();

            Create.UniqueConstraint("uq_alloy_chemical_composition")
                .OnTable("alloy_chemical_compositions")
                .Columns("alloy_grade_id", "chemical_element_id");

            Create.Index("ix_alloy_grades_alloy_system_id").OnTable("alloy_grades").OnColumn("alloy_system_id");
            Create.Index("ix_alloy_chemical_compositions_alloy_grade_id").OnTable("alloy_chemical_compositions").OnColumn("alloy_grade_id");
            Create.Index("ix_alloy_chemical_compositions_chemical_element_id").OnTable("alloy_chemical_compositions").OnColumn("chemical_element_id");
        }
    }
}
