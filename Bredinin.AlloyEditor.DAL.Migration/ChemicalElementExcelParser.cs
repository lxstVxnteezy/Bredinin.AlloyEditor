using Bredinin.MyPetProject.Domain.Dictionaries;
using OfficeOpenXml;

namespace Bredinin.MyPetProject.DAL.Migration
{
    internal static class ChemicalElementExcelParser
    {
        public static List<DictChemicalElement> ParseElements()
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Dictionaries",
                "ChemicalElements.xlsx"
            );

            var elements = new List<DictChemicalElement>();

            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var atomicWeightText = worksheet.Cells[row, 6].Text.Replace(".", ",");
                var density = worksheet.Cells[row, 6].Text.Replace(".", ",");

                elements.Add(new DictChemicalElement
                {
                    Id = Guid.NewGuid(),
                    Symbol = worksheet.Cells[row, 1].Text,
                    Name = worksheet.Cells[row, 2].Text,
                    IsBaseForAlloySystem = bool.Parse(worksheet.Cells[row, 3].Text),
                    Description = worksheet.Cells[row, 4].Text,
                    AtomicNumber = int.Parse(worksheet.Cells[row, 5].Text),
                    AtomicWeight = decimal.Parse(atomicWeightText),
                    Density = decimal.Parse(density),
                    Group = int.Parse(worksheet.Cells[row, 7].Text),
                    Period = int.Parse(worksheet.Cells[row, 8].Text),
                });
            }

            return elements;
        }
    }
}
