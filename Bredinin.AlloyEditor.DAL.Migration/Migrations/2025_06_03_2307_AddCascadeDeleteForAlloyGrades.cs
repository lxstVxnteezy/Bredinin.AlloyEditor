using System.Data;
using FluentMigrator;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202503062307)]
    public class AddCascadeDeleteForAlloyGrades_03_06_2025_2307 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Delete.ForeignKey("fk_compositions_alloy_grades")
                .OnTable("alloy_chemical_compositions");

            Create.ForeignKey("fk_compositions_alloy_grades")
                .FromTable("alloy_chemical_compositions").ForeignColumn("alloy_grade_id")
                .ToTable("alloy_grades").PrimaryColumn("id")
                .OnDelete(Rule.Cascade); 
        }
    }
}
