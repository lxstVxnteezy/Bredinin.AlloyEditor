using FluentMigrator;


namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202603061050)]
    public class AddFieldsToDictTypeOfHeatTreatment_2026_03_06_1050 : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("dict_types_of_heat_treatments")
                .AddColumn("code").AsString(50).Nullable()
                .AddColumn("default_temperature_min").AsInt32().Nullable()
                .AddColumn("default_temperature_max").AsInt32().Nullable()
                .AddColumn("default_cooling_medium").AsString(100).Nullable();

            if (Schema.Table("dict_types_of_heat_treatments").Column("descrpition").Exists())
            {
                Rename.Column("descrpition").OnTable("dict_types_of_heat_treatments")
                    .To("description");
            }

            Create.Index("ix_dict_heat_treatment_code")
                .OnTable("dict_types_of_heat_treatments")
                .OnColumn("code");
        }
    }
}
