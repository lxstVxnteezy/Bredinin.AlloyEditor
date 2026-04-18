using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202603062256)]
    public class FixMechanicalPropertiesUniqueConstraint_2026_03_06_2256 : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Удаляем старый некорректный constraint (если он был создан)
            Delete.UniqueConstraint("uq_alloy_mechanical_property")
                .FromTable("alloy_mechanical_properties");

            // Создаем правильные partial unique indexes
            Execute.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS uq_alloy_mechanical_property_with_treatment
                ON alloy_mechanical_properties (alloy_grade_id, property_type_id, heat_treatment_id)
                WHERE heat_treatment_id IS NOT NULL;
            ");

            Execute.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS uq_alloy_mechanical_property_no_treatment
                ON alloy_mechanical_properties (alloy_grade_id, property_type_id)
                WHERE heat_treatment_id IS NULL;
            ");
        }
    }
}