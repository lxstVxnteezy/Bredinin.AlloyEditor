using System.Globalization;
using Bredinin.AlloyEditor.DAL.Migration.Parsers;
using FluentMigrator;
using Npgsql;

namespace Bredinin.AlloyEditor.DAL.Migration.Migrations
{
    [Migration(202504060146)]
    public class AddDataForChemicalDict_04_06_2025_0146 : ForwardOnlyMigration
    {
        public override void Up()
        {
            var elements = ChemicalElementExcelParser.ParseElements();
            var culture = CultureInfo.InvariantCulture;
            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = (NpgsqlConnection)connection;
                var npgsqlTransaction = (NpgsqlTransaction)transaction;

                using (var cmd = new NpgsqlCommand(@"
                    CREATE TEMPORARY TABLE temp_chemical_elements (
                        LIKE dict_chemical_elements INCLUDING DEFAULTS
                    ) ON COMMIT DROP", npgsqlConnection, npgsqlTransaction))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var writer = npgsqlConnection.BeginTextImport(@"
                    COPY temp_chemical_elements 
                    (id, name, symbol, description, is_base_for_alloy_system, 
                     atomic_number, atomic_weight, ""group"", period, density) 
                    FROM STDIN"))
                {
                    foreach (var element in elements)
                    {
                        writer.Write(Guid.NewGuid().ToString()); 
                        writer.Write("\t");
                        writer.Write(element.Name ?? "");       
                        writer.Write("\t");
                        writer.Write(element.Symbol ?? "");      
                        writer.Write("\t");
                        writer.Write(element.Description ?? ""); 
                        writer.Write("\t");
                        writer.Write(element.IsBaseForAlloySystem.ToString().ToLower());
                        writer.Write("\t");
                        writer.Write(element.AtomicNumber.ToString(culture));
                        writer.Write("\t");

                        writer.Write(element.AtomicWeight.ToString(culture)); 
                        writer.Write("\t");
                        writer.Write(element.Group.ToString(culture)); 
                        writer.Write("\t");
                        writer.Write(element.Period.ToString(culture)); 
                        writer.Write("\t");

                        writer.Write(element.Density.ToString(culture)); 
                        writer.WriteLine();
                    }
                }

                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO dict_chemical_elements 
                    SELECT * FROM temp_chemical_elements",
                    npgsqlConnection, npgsqlTransaction))
                {
                    cmd.ExecuteNonQuery();
                }
            });
        }
    }
}