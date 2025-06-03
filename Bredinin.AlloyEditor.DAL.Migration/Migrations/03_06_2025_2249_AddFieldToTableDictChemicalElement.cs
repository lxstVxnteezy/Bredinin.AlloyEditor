using FluentMigrator;

namespace Bredinin.MyPetProject.DAL.Migration.Mirgations;

[Migration(202503062249)]
public class AddFieldToTableDictChemicalElement_03_06_2025_2249 : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("dict_chemical_elements")
            .AddColumn("atomic_number").AsInt32().NotNullable()
            .AddColumn("atomic_weight").AsDecimal().NotNullable()
            .AddColumn("group").AsInt16().NotNullable()
            .AddColumn("period").AsInt16().NotNullable()
            .AddColumn("density").AsDecimal().NotNullable();
    }
}